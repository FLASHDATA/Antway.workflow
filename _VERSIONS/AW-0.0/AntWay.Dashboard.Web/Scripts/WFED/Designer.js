var WorkflowDesignerCommon = {
    modal: function(obj, p){
        if(obj.semanticmodal != undefined)
            obj.semanticmodal(p);
        else if(obj.modal != undefined)
            obj.modal(p);
        else
            console.error("SemanticUI is not defined!");
    },
    createArrowByAngle: function (x, y, angle, headlen, colour) {

        if (colour == undefined)
            colour = 'red';

        var wedge = new Konva.Wedge({
            x: x,
            y: y,
            radius: headlen,
            angle: 40,
            fill: colour,
            rotation: angle * 180 / Math.PI - 200
        });

        return wedge;
    },
    updateArrowByAngle: function (arrow, x, y, angle, headlen, colour) {

        if (colour == undefined)
            colour = 'red';

        arrow.setPosition({ x: x, y: y });
        arrow.setRadius(headlen);
        arrow.setFill(colour);
        arrow.setRotation(angle * 180 / Math.PI - 200);
    },
    createUUID: function () {

        var s = [];
        var hexDigits = "0123456789abcdef";
        for (var i = 0; i < 36; i++) {
            s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
        }
        s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
        s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
        s[8] = s[13] = s[18] = s[23] = "-";

        var uuid = s.join("");
        return uuid;
    },
    DataCorrection: function (data) {
        if (data.AdditionalParams == undefined)
            data.AdditionalParams = {};

        if (data.AdditionalParams.Actions == undefined)
            data.AdditionalParams.Actions = [];

        if (data.AdditionalParams.Conditions == undefined)
            data.AdditionalParams.Conditions = [];

        if (data.AdditionalParams.Rules == undefined)
            data.AdditionalParams.Rules = [];

        var checkAdditionalParams = function (item, params) {
            if (item == undefined)
                return;

            var findAction = $.grep(params, function (el) {
                return el == item;
            });

            if (findAction.length == 0)
                params.push(item);
        };

        var checkLink = function (value, array, propertyName) {
            if (value == undefined || array == undefined)
                return;

            var findItems = $.grep(array, function (el) {
                return value == el[propertyName];
            });

            if (findItems.length > 0){
                return findItems[0];
            }
        };

        data.Activities.forEach(function (a) {
            if (a.Implementation != undefined) {
                a.Implementation.forEach(function (ai) {
                    checkAdditionalParams(ai.Name, data.AdditionalParams.Actions);
                });
                //a.PreExecutionImplementation.forEach(function (ai) {
                //    checkAdditionalParams(ai.Name, data.AdditionalParams.Actions);
                //});
            }
        });

        data.Transitions.forEach(function (t) {

            if (t.From != undefined) {
                t.From = checkLink(t.From.Name, data.Activities, 'Name');
            }

            if (t.To != undefined) {
                t.To = checkLink(t.To.Name, data.Activities, 'Name');
            }

            if (t.Restrictions != undefined) {
                t.Restrictions.forEach(function (cip) {
                    cip.Actor = checkLink(cip.Actor.Name, data.Actors, 'Name');
                });
            }

            if (t.Condition != undefined && t.Condition.Action != undefined) {
                checkAdditionalParams(t.Condition.Action.Name, data.AdditionalParams.Actions);                
            }

            if (t.Trigger != undefined && t.Trigger.Command != undefined) {
                t.Trigger.Command = checkLink(t.Trigger.Command.Name, data.Commands, 'Name');
            }

            if (t.Trigger != undefined && t.Trigger.Timer != undefined) {
                t.Trigger.Timer = checkLink(t.Trigger.Timer.Name, data.Timers, 'Name');
            }
        });

        data.Commands.forEach(function (c) {
            if (c.InputParameters != undefined) {
                c.InputParameters.forEach(function (cip) {
                    cip.Parameter = checkLink(cip.Parameter.Name, data.Parameters, 'Name');
                });
            }
        });

        data.Actors.forEach(function (a) {
            if (a.Rule != undefined) {
                checkAdditionalParams(a.Rule, data.AdditionalParams.Rules);
            }
        });

    },
    download: function (url, data, method) {
        if (url && data) {
            var inputs = new Array();
            data.forEach(function (item) {
                var tmp = $('<input type="hidden"/>');
                tmp.attr('name', item.name);
                tmp.attr('value', item.value);
                inputs.push(tmp);
            });

            var form = $('<form action="' + url + '" method="' + (method || 'post') + '"></form>');
            form.append(inputs);
            form.appendTo('body').submit().remove();
        };
    },
    defineLocalStorage: function(){
        Object.defineProperty(window, "localStorage", new (function () {
            var aKeys = [], oStorage = {};
            Object.defineProperty(oStorage, "getItem", {
            value: function (sKey) { return sKey ? this[sKey] : null; },
            writable: false,
            configurable: false,
            enumerable: false
            });
            Object.defineProperty(oStorage, "key", {
            value: function (nKeyId) { return aKeys[nKeyId]; },
            writable: false,
            configurable: false,
            enumerable: false
            });
            Object.defineProperty(oStorage, "setItem", {
            value: function (sKey, sValue) {
                if(!sKey) { return; }
                document.cookie = escape(sKey) + "=" + escape(sValue) + "; expires=Tue, 19 Jan 2038 03:14:07 GMT; path=/";
            },
            writable: false,
            configurable: false,
            enumerable: false
            });
            Object.defineProperty(oStorage, "length", {
            get: function () { return aKeys.length; },
            configurable: false,
            enumerable: false
            });
            Object.defineProperty(oStorage, "removeItem", {
            value: function (sKey) {
                if(!sKey) { return; }
                document.cookie = escape(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
            },
            writable: false,
            configurable: false,
            enumerable: false
            });    
            Object.defineProperty(oStorage, "clear", {
            value: function () {
                if(!aKeys.length) { return; }
                for (var sKey in aKeys) {
                document.cookie = escape(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
                }
            },
            writable: false,
            configurable: false,
            enumerable: false
            });
            this.get = function () {
            var iThisIndx;
            for (var sKey in oStorage) {
                iThisIndx = aKeys.indexOf(sKey);
                if (iThisIndx === -1) { oStorage.setItem(sKey, oStorage[sKey]); }
                else { aKeys.splice(iThisIndx, 1); }
                delete oStorage[sKey];
            }
            for (aKeys; aKeys.length > 0; aKeys.splice(0, 1)) { oStorage.removeItem(aKeys[0]); }
            for (var aCouple, iKey, nIdx = 0, aCouples = document.cookie.split(/\s*;\s*/); nIdx < aCouples.length; nIdx++) {
                aCouple = aCouples[nIdx].split(/\s*=\s*/);
                if (aCouple.length > 1) {
                oStorage[iKey = unescape(aCouple[0])] = unescape(aCouple[1]);
                aKeys.push(iKey);
                }
            }
            return oStorage;
            };
            this.configurable = false;
            this.enumerable = true;
        })());
    },
    imageCache: [],
    loadImage: function(src, func){
        var image;
        for(var i=0; i < WorkflowDesignerCommon.imageCache.length; i++){
           var img = WorkflowDesignerCommon.imageCache[i];
           if(img.src == src){
               image = img;
               break;
           }
        }
        
        if(image != undefined){
            if(func != undefined)
                func(image);
            return image;
        }

        image = new Image();
        if(func != undefined){
            image.onload = function(){
                func(image);
            };
        }
        image.src = src;
        return image;
    }
};

function WorkflowDesigner(settings) {
    var me = this;
    this.Settings = settings;

    if (!window.localStorage) {
        WorkflowDesignerCommon.defineLocalStorage();
    }

    this.GetName = function () { return me.Settings.name; };

    this.error = function (msg) {
        alert(msg);
    };

    this.refresh = function(){
        var lp = (this.data !== undefined && this.data.__loadParams !== undefined) 
            ?  this.data.__loadParams : this.loadParams;
        if(lp === undefined){
            alert('You might use refresh method after called load method only.');
        }
        else{
           this.load(lp);
        }
    };

    this.getParam = function(name){
        return localStorage["WorkflowDesigner_" + name];
    };

    this.setParam = function(name, value){
        var key = "WorkflowDesigner_" + name;
        localStorage[key] = value;
    };

    this.load = function (params) {
        var data = new Array();
        this.loadParams = params;
        this.schemecode = params.schemecode;
        this.processid = params.processid;
        this.schemeid = params.schemeid;
        if (params.readonly){ //Only for compartibility
            this.Settings.readonly = params.readonly;
        }

        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'load' });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            success: function (response) {
                var data = {};
                try {

                    data = JSON.parse(response);
                }
                catch (exception) {
                    me.error(response);
                    return;
                }

                if (data.isError){
                    me.error(data.errorMessage);
                    return;
                }
                data.__loadParams = params;
                me.data = data;
                me.render();
            },
            error: function (request, textStatus, errorThrown) {
                me.error(textStatus + ' ' + errorThrown);
            }
        });
    };

    this.exists = function (params) {
        var data = new Array();

        this.schemecode = params.schemecode;
        this.processid = params.processid;
        this.schemeid = params.schemeid;
        if (params.readonly)
            this.Settings.readonly = params.readonly;

        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'exists' });

       var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: false,
            error: function (request, textStatus, errorThrown) {
                me.error(textStatus + ' ' + errorThrown);
            }
       }).responseText;

        try{
            return JSON.parse(res);
        }
        catch (exception) {
            me.error(res);
            return false;
        }

    };

    this.create = function () {
        var data = new Array();
        data.push({ name: 'operation', value: 'load' });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            success: function (response) {
                try{
                    me.data = JSON.parse(response);
                }
                catch (exception) {
                    me.error(response);
                }
                me.render();
            }
        });
    };

    this.render = function () {
        var isFullscreeen = false;
        if (me.Graph){
            me.Graph.destroy();

            //restore state
            if(me.data != undefined && me.data.__loadParams != undefined){
                if(me.data.__loadParams.isFullScreen != undefined){
                    isFullscreeen = me.data.__loadParams.isFullScreen;
                }
    
                if(me.data.__loadParams.readonly != undefined){
                    this.Settings.readonly = me.data.__loadParams.readonly;
                }
            }
        }
        
        var components = [
            WorkflowDesignerActivityManager,
            WorkflowDesignerTransitionManager
            //WorkflowDesignerKeyboard
        ];

        // if (this.Settings.showoverviewmap) {
        //     components.push(WorkflowDesignerOverviewMap);
        // }

        if (!this.Settings.printable) {
            components.push(WorkflowDesignerBackground);
        };
        
        if (!this.Settings.notrendertoolbar) {
            components.push(WorkflowDesignerToolbar);
        };

        if (me.Settings.printable && me.data != undefined)
        {
            var maxX = 0;
            var maxY = 0;
            $(me.data.Activities).each(function (index){
                var x = parseInt(this.DesignerSettings.X);
                var y = parseInt(this.DesignerSettings.Y);
                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
               });
            me.Settings.graphwidth = maxX + me.Settings.DefaultActivityWidth;
            me.Settings.graphheight = maxY + me.Settings.DefaultActivityHeight;
        }

        me.Graph = new WorkflowGraph(this.Settings.renderTo, me, me.Settings, components);
        me.Graph.setFullScreen(isFullscreeen);

        if (me.data != undefined)
        {
            WorkflowDesignerCommon.DataCorrection(me.data);
            if(me.data.__loadParams != undefined && me.data.__loadParams.graphData != undefined){
                me.Graph.graphData = me.data.__loadParams.graphData;
                me.Graph.graphDataIndex = me.data.__loadParams.graphDataIndex;
            }
            me.Graph.Draw(me.data);
        }
    };

    this.save = function (successFunc) {

        if (me.Settings.readonly) {
            alert(WorkflowDesignerConstants.ErrorReadOnlySaveText);
            return;
        }

        var data = new Array();
        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });        
        data.push({ name: 'operation', value: 'save' });
        data.push({ name: 'data', value: JSON.stringify(this.data) });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                var data = {};
                try {

                    data = JSON.parse(response);
                }
                catch (exception) {
                    me.error(response);
                    return;
                }

                if (data.isError){
                    me.error(data.errorMessage);
                    return;
                }

                me.data = data;
                me.render();

                if (successFunc) {
                    setTimeout(function() {successFunc(me);}, 100);
                }
            }
        });
    };

    this.downloadscheme = function (params) {
        var data = new Array();
        data.push({ name: 'operation', value: 'downloadscheme' });
        data.push({ name: 'data', value: JSON.stringify(this.data) });
        WorkflowDesignerCommon.download(this.Settings.apiurl, data, 'post');
    };

    this.uploadscheme = function (form, successFunc) {

        var iframeid = this.GetName() + '_uploadiframe';

        // Create the iframe...
        var iframe = document.createElement("iframe");
        iframe.setAttribute("id", iframeid);
        iframe.setAttribute("name", iframeid);
        iframe.setAttribute("width", "0");
        iframe.setAttribute("height", "0");
        iframe.setAttribute("border", "0");
        iframe.setAttribute("style", "width: 0; height: 0; border: none;");

        // Add to document...
        form.parentNode.appendChild(iframe);
        window.frames[iframeid].name = iframeid;

        var iframeById = document.getElementById(iframeid);

        // Add event...
        var eventHandler = function () {

            if (iframeById.detachEvent) iframeById.detachEvent("onload", eventHandler);
            else iframeById.removeEventListener("load", eventHandler, false);

            // Message from server...
            if (iframeById.contentDocument) {
                //content = iframeById.contentDocument.body.innerHTML;
                content = iframeById.contentDocument.body.innerText;
            } else if (iframeById.contentWindow) {
                content = iframeById.contentWindow.document.body.innerHTML;
            } else if (iframeById.document) {
                content = iframeById.document.body.innerHTML;
            }

            // Del the iframe...
            setTimeout(function () {iframeById.parentNode.removeChild(iframeById)}, 250);

            var data = {};
            try
            {
                data = JSON.parse(content);
            }
            catch (exception) {
                me.error(content);
                return;
            }

            if (data.isError){
                me.error(data.errorMessage);
                return;
            }

            me.data = data;
            me.render();

            if (successFunc)
                successFunc(me);
        }

        if (iframeById.addEventListener) iframeById.addEventListener("load", eventHandler, true);
        if (iframeById.attachEvent) iframeById.attachEvent("onload", eventHandler);

        form.setAttribute("target", iframeid);
        form.setAttribute("action", this.createurl('uploadscheme'));
        form.setAttribute("method", "post");
        form.setAttribute("enctype", "multipart/form-data");
        form.setAttribute("encoding", "multipart/form-data");

        form.submit();
    };

    this.createurl = function(operation){
        var url = this.Settings.apiurl;
        var separator = '?';
        if (url.indexOf('?') >= 0)
            separator = '&';

        url += separator + "operation=" + operation;
        separator = '&';

        if (this.schemeid != undefined) {
            url += separator + "schemeid=" + this.schemeid;
        }
       
        if (this.processid != undefined) {
            url += separator + "processid=" + this.processid;
        }

        if (this.schemecode != undefined) {
            url += separator + "schemecode=" + this.schemecode;
        }

        return url;
    };

    this.validate = function () {
        var err = undefined;
        
        var findActivityInitilal = $.grep(me.data.Activities, function (el) {
            return el.IsInitial == true;
        });

        if (findActivityInitilal.length != 1)
            err = WorkflowDesignerConstants.ErrorActivityIsInitialCountText;

        return err;
    };

    this.destroy = function () {
        this.schemecode = undefined;
        this.processid = undefined;
        this.schemeid = undefined;
        this.data = undefined;
        this.Graph.destroy();
    }

    this.compile = function (item, successFunc) {
        item = { Name: item.Name, Type: item.Type, IsGlobal: item.IsGlobal, IsAsync: item.IsAsync, ActionCode: item.ActionCode, Usings: item.Usings };
        var data = new Array();
        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'compile' });
        data.push({ name: 'data', value: JSON.stringify(item) });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                try {
                    response = JSON.parse(response);
                }
                catch (exception) {
                    me.error(response);
                }

                if (successFunc) {
                    setTimeout(function () { successFunc(response); }, 100);
                }
            }
        });
    }

    this.deleteGlobalCodeAction = function (names, successFunc) {
        var data = new Array();
        data.push({ name: 'operation', value: 'deleteglobalcodeaction' });
        data.push({ name: 'names', value: JSON.stringify(names) });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                try {
                    response = JSON.parse(response);
                }
                catch (exception) {
                    me.error(response);
                }

                if (successFunc) {
                    setTimeout(function () { successFunc(response); }, 100);
                }
            }
        });
    }

    this.getemptytype = function (item, successFunc) {
          
            var data = new Array();
            data.push({ name: 'operation', value: 'getemptytype' });
            data.push({ name: 'data', value: JSON.stringify(item) });

            var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                
                if (successFunc) {
                    setTimeout(function () { successFunc(response); }, 100);
                }
            }
        });
    }

    this.requestautocompletesuggestions = function (category, value) {

        var data = new Array();
        data.push({ name: 'operation', value: 'getautocompletesuggestions' });
        data.push({ name: 'category', value: category });
        data.push({ name: 'value', value: value });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: false,
            type: "post"
        });

        return JSON.parse(res.responseText);
    }

    this.readonlymode = function (settings) {
        var me = this;
        if (settings === undefined || settings == null)
        {
            me.Settings.notrendertoolbar = false;
            me.Settings.notshowwindows = false;
            me.Settings.disableobjectmovements = false;
        } else {
            if (settings.notrendertoolbar != undefined){
                me.Settings.notrendertoolbar = settings.notrendertoolbar;
            } else {
                me.Settings.notrendertoolbar = false;
            }

            if (settings.notshowwindows != undefined){
                me.Settings.notshowwindows = settings.notshowwindows;
            } else {
                me.Settings.notshowwindows = false;
            }

            if (settings.disableobjectmovements != undefined){
                me.Settings.disableobjectmovements = settings.disableobjectmovements;
            } else {
                me.Settings.disableobjectmovements = false;
            }
        }
        me.Settings.readonly = true;
        if (me.Settings.printable)
        {
            me.Settings.graphheight = me.Settings.originalgraphheighth;
            me.Settings.graphwidth = me.Settings.originalgraphwidth;
            me.Settings.printable = false;
        }
        me.render();
    }

    this.printablemode = function () {
        var me = this;
        if (!me.Settings.printable){
            me.Settings.originalgraphheighth = me.Settings.graphheight;
            me.Settings.originalgraphwidth = me.Settings.graphwidth;
        }
        me.Settings.notrendertoolbar = true;
        me.Settings.notshowwindows = true;
        me.Settings.disableobjectmovements = false;
        me.Settings.readonly = true;
        me.Settings.printable = true;
        me.render();
    }

    this.editablemode = function () {
        var me = this;
        me.Settings.notrendertoolbar = false;
        me.Settings.notshowwindows = false;
        me.Settings.disableobjectmovements = false;
        me.Settings.readonly = false;
        if (me.Settings.printable)
        {
            me.Settings.graphheight = me.Settings.originalgraphheighth;
            me.Settings.graphwidth = me.Settings.originalgraphwidth;
            me.Settings.printable = false;
        }

        me.render();
    }   

    if (settings.notrendertoolbar != undefined)
        this.Settings.notrendertoolbar = settings.notrendertoolbar;
    if (settings.notshowwindows != undefined)
        this.Settings.notshowwindows = settings.notshowwindows;
    if (settings.disableobjectmovements != undefined)
        this.Settings.disableobjectmovements = settings.disableobjectmovements;

    if (this.Settings.mode === undefined)
    {
        this.editablemode();
    }
    else if (this.Settings.mode.toLowerCase() === "readonly")
    {
        this.readonlymode(settings);
    }
    else if (this.Settings.mode.toLowerCase() === "printable")
    {
        this.printablemode();
    }
    else {
        this.editablemode();
    } 

    this.autoarrangement = function () {
        var toolbar = this.Graph.GetComponentByType("WorkflowDesignerToolbar");
        if (toolbar != undefined)
            toolbar.AutoArrangement();
    }
    this.downloadschemeBPMN = function (params) {
        var data = new Array();
        data.push({ name: 'operation', value: 'downloadschemebpmn' });
        data.push({ name: 'data', value: JSON.stringify(this.data) });
        WorkflowDesignerCommon.download(this.Settings.apiurl, data, 'post');
    };

    this.uploadschemeBPMN = function (form, successFunc) {

        var iframeid = this.GetName() + '_uploadiframe';

        // Create the iframe...
        var iframe = document.createElement("iframe");
        iframe.setAttribute("id", iframeid);
        iframe.setAttribute("name", iframeid);
        iframe.setAttribute("width", "0");
        iframe.setAttribute("height", "0");
        iframe.setAttribute("border", "0");
        iframe.setAttribute("style", "width: 0; height: 0; border: none;");

        // Add to document...
        form.parentNode.appendChild(iframe);
        window.frames[iframeid].name = iframeid;

        var iframeById = document.getElementById(iframeid);

        // Add event...
        var eventHandler = function () {

            if (iframeById.detachEvent) iframeById.detachEvent("onload", eventHandler);
            else iframeById.removeEventListener("load", eventHandler, false);

            // Message from server...
            if (iframeById.contentDocument) {
                //content = iframeById.contentDocument.body.innerHTML;
                content = iframeById.contentDocument.body.innerText;
            } else if (iframeById.contentWindow) {
                content = iframeById.contentWindow.document.body.innerHTML;
            } else if (iframeById.document) {
                content = iframeById.document.body.innerHTML;
            }

            // Del the iframe...
            setTimeout(function () { iframeById.parentNode.removeChild(iframeById) }, 250);

            var data = {};
            try {
                data = JSON.parse(content);
            }
            catch (exception) {
                me.error(content);
                return;
            }

            if (data.isError) {
                me.error(data.errorMessage);
                return;
            }

            me.data = data;
            me.render();

            if (successFunc)
                successFunc(me);
        }

        if (iframeById.addEventListener) iframeById.addEventListener("load", eventHandler, true);
        if (iframeById.attachEvent) iframeById.attachEvent("onload", eventHandler);

        form.setAttribute("target", iframeid);
        form.setAttribute("action", this.createurl('uploadschemebpmn'));
        form.setAttribute("method", "post");
        form.setAttribute("enctype", "multipart/form-data");
        form.setAttribute("encoding", "multipart/form-data");

        form.submit();
    };
};
