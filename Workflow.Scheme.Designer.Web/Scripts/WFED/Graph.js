function WorkflowGraph(container, designer, settings, components) {
    var me = this;
    me.container = container;
    me.designer = designer;
   
    me.suggestionsCache = new Object();
    
    if (settings == undefined) {
        settings = new Object();
    }
    
    if (settings.Container == undefined) {
        settings.Container = 'container';
    }

    if (settings.graphwidth == undefined) {
        settings.graphwidth = 1024;
    }

    if (settings.graphheight == undefined) {
        settings.graphheight = 768;
    }

    if (settings.DefaultActivityWidth == undefined) {
        settings.DefaultActivityWidth = 200;
    }

    if (settings.DefaultActivityHeight == undefined) {
        settings.DefaultActivityHeight = 60;
    }

    if (settings.DefaultMoveStep == undefined) {
        settings.DefaultMoveStep = 10;
    }

    if (settings.imagefolder == undefined)
        settings.imagefolder = '/images/';

    this.Settings = settings;
    this.Settings.ContainerStage = this.container + '_stage';
    $('#' + this.container).append('<div id=\'' + this.Settings.ContainerStage + '\' class=\'workflowenginecontainerstage\'></div>');

    this.Stage = new Konva.Stage({
        container: this.Settings.ContainerStage,
        width: parseInt(this.Settings.graphwidth),
        height: parseInt(this.Settings.graphheight)
    });

    this.getParam = function(name){
       var v = this.designer.getParam(name);
       if(v === "true") return true;
       else if(v === "false") return false;

       return v;
    };

    this.setParam = function(name, value){
        this.designer.setParam(name, value);
    };

    //Components
    this.Components = new Array();
    this.AddComponent = function (componentclass) {
        var obj = new componentclass();
        obj.init(this);
        me.Components.push(obj);
        return obj;
    };
    this.GetComponentByType = function (type) {
        for (var i = 0; i < this.Components.length; i++) {
            if (this.Components[i].type == type)
                return this.Components[i];
        }
        return undefined;
    };
    this.ComponentsExecute = function (func, params) {
        me.Components.forEach(function (item) {
            if (item[func])
                item[func](params);
        });
    };

    if (components) {
        components.forEach(function (item) {
            me.AddComponent(item);
        });
    }

    this.Draw = function (data) {
        me.data = data;
        
        if(me.graphData.length == 0){
            me.StoreGraphData();
        }

        me.onSelectionChanged();
        me.ComponentsExecute('draw');
    };

    this.GraphLayerSetOffset = function (x, y) {
        me.ComponentsExecute('LayerSetOffset', { x: x, y: y });
        me.redrawAll();
    };

    this.GraphLayerScale = function (a) {
        me.ComponentsExecute('LayerScale', a);
        me.redrawAll();
    };
    this.GraphLayerScaleNorm = function () {
        me.ComponentsExecute('LayerScaleNorm');
        me.redrawAll();
    };

    this.Refresh = function () {
        me.designer.refresh();
    };

    this.onFullScreenClick = function(){
        if(this.data.__loadParams !== undefined && this.data.__loadParams.isFullScreen){
            this.setFullScreen(false);
            this.data.__loadParams.isFullScreen = false;
        }
        else{
            this.setFullScreen(true);
            if (this.data.__loadParams === undefined)
                this.data.__loadParams = {};
            this.data.__loadParams.isFullScreen = true;
        }
        
        this.redrawAll();
    };

    this.setFullScreen = function (isFullScreen) {
        var el = $('#' + this.container);

        if(this._toolbar == undefined)
            this._toolbar = this.GetComponentByType("WorkflowDesignerToolbar");

        var toolbar = this._toolbar;
        if(isFullScreen){
            this.originalContainerStyle = el.attr('style');
            if(this.originalContainerStyle == undefined)
                this.originalContainerStyle = "";
            
            this.originalWidth = this.Stage.width();
            this.originalHeight = this.Stage.height();
 
            var width = $(window).width() - 2;
            var height = $(window).height() - 2;

            this.Stage.hide();
            this.Stage.width(width);
            this.Stage.height(height);

            if(toolbar != undefined){
                toolbar.setItemActive('fullscreen', true);
                toolbar.changeWidth(width);
            }

            el.css({
                position: 'absolute',
                top: 0,
                left: 0,
                width: width,
                height: height,
                "z-index": 10000,
                background: 'white',
            });

            this.Stage.show();
        }
        else if(this.originalContainerStyle != undefined && this.originalWidth != undefined && this.originalHeight != undefined) {
            this.Stage.hide();
            el.attr('style', this.originalContainerStyle);
            this.Stage.width(this.originalWidth);
            this.Stage.height(this.originalHeight);

            if(toolbar != undefined){
                toolbar.setItemActive('fullscreen', false);
                toolbar.changeWidth(this.originalWidth);
            }

            this.Stage.show();
        }
    };

    this.DeselectAll = function () {
        me.ComponentsExecute('DeselectAll');
        me.onSelectionChanged(false);
        me.redrawAll();
        
    };
    this.redrawAll = function () {
        me.Stage.batchDraw();
    };
    this.CorrectPossition = function (e, layer) {
        if (layer.getScaleX() == 0 || layer.getScaleY() == 0)
            return { x: layer.getOffsetX(), y: 0 };

        return {
            x: e.x / layer.getScaleX() + layer.getOffsetX(),
            y: e.y / layer.getScaleY() + layer.getOffsetY()
        }
    };

    this.DeleteSelected = function () {
        var me = this;
        var array = new Array();

        this.Components.forEach(function (item) {
            if (item.GetSelected)
                array = array.concat(item.GetSelected());
        });

        if (array.length > 0) {
            me.confirm(WorkflowDesignerConstants.DeleteConfirm, function(){
                array.forEach(function (item) {
                    item.Delete();
                });

                me.onSelectionChanged(false);
                me.redrawAll();
               
            });
        }
    };

    this.confirm = function (text, success) {
        var w = $('<div class="ui mini modal"></div>');
        w.append($('<div class="content"><p>' + text + '</p></div>'));

        var actions = $('<div class="actions"></div>')
            .append('<div class="ui primary ok button">'+ WorkflowDesignerConstants.ButtonTextYes +'</div>')
            .append('<div class="ui secondary  cancel button">'+ WorkflowDesignerConstants.ButtonTextCancel +'</div>');
   
        w.append(actions);
        WorkflowDesignerCommon.modal(w,{
            onApprove : function() {
                success();
              }
        });
        WorkflowDesignerCommon.modal(w, "show");
    }

    this.destroy = function () {
        if(this.originalContainerStyle != undefined){
            var el = $('#' + this.container);
            el.attr('style', this.originalContainerStyle);
        }

        if(this.data != undefined){
            if(this.data.__loadParams == undefined)
                this.data.__loadParams = {};
            this.data.__loadParams.graphData = this.graphData;
            this.data.__loadParams.graphDataIndex = this.graphDataIndex;
        }
        this.Stage.destroy();
    };

    this.GetCurrentActivity = function () {
        if (me.data == undefined || me.data.AdditionalParams == undefined || me.data.AdditionalParams.ProcessParameters == undefined) {
            return undefined;
        }

        for (var i = 0; i < me.data.AdditionalParams.ProcessParameters.length; i++)
        {
            var item = me.data.AdditionalParams.ProcessParameters[i];
            if (item.Name === 'CurrentActivity')
                return item.Value;
        }
        return undefined;
    };

    this.isCurrentActivityForSubprocess = function (activityName) {
        if (me.data == undefined || me.data.AdditionalParams == undefined || !Array.isArray(me.data.AdditionalParams.SubprocessCurrentActivities)) {
            return false;
        }

        for (var i = 0; i < me.data.AdditionalParams.SubprocessCurrentActivities.length; i++)
        {   
            if (activityName === me.data.AdditionalParams.SubprocessCurrentActivities[i])
                return true;
        }
        return false;
    };

    this.getActionNames = function () {
        var me = this;
        var codeActions = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if (codeAction.Type.toLowerCase() === 'action')
                codeActions.push(codeAction.Name);
        }
        return me.data.AdditionalParams.Actions.concat(codeActions);
    },

    this.getConditionNames = function () {
        var me = this;
        var codeActions = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if (codeAction.Type.toLowerCase() === 'condition')
                codeActions.push(codeAction.Name);
        }
        return me.data.AdditionalParams.Conditions.concat(codeActions);
    }

    this.getActorNames = function () {
        var me = this;
        var actors = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if ((codeAction.Type.toLowerCase() === 'ruleget') || (codeAction.Type.toLowerCase() === 'rulecheck'))
                actors.push(codeAction.Name);
        }
        return me.unique(me.data.AdditionalParams.Rules.concat(actors));
    }

    this.getTypeNames = function () {
        var me = this;
        var types = new Array();
        for (var i = 0; i < me.data.Parameters.length; i++) {
            var parameter = me.data.Parameters[i];
            types.push(decodeURIComponent(parameter.Type));
        }
        return me.unique(me.data.AdditionalParams.Types.concat(types));
    }



    this.getAutoCompleteSuggestions = function (category, value, query) {
        var suggestions;
        if (value === undefined || value === "")
            return new Array();
        if (this.suggestionsCache[category] != undefined && this.suggestionsCache[category][value] != undefined) {
            suggestions = this.suggestionsCache[category][value];
        } else {
            if (this.suggestionsCache[category] == undefined) {
                this.suggestionsCache[category] = new Object();
            }
            suggestions = this.designer.requestautocompletesuggestions(category, value);
            this.suggestionsCache[category][value] = suggestions;
        }
        return $.grep(suggestions, function (el) {
            return el.toLowerCase().indexOf(query.toLowerCase()) >= 0;
        });;
    }

    this.getNonSystemParameters = function () {
        var me = this;
        var parameters = new Array();
        for (var i = 0; i < me.data.Parameters.length; i++) {
            var parameter = me.data.Parameters[i];
            if ((parameter.Purpose.toLowerCase() != 'system'))
            {
                parameters.push(parameter);
            }
        }
        return parameters;
    };

    this.unique = function(array) {
        return $.grep(array, function (el, index) {
            return index === $.inArray(el, array);
        });
    };

    //Undo/Redo
    this.graphData = [];
    this.graphDataIndex = -1;

    this.StoreGraphData = function(){
        var gd = undefined;
        if(this.data.__loadParams != undefined){
            gd = this.data.__loadParams.graphData;
            this.data.__loadParams.graphData = undefined;
        }

        var tmp = JSON.stringify(this.data);
        if(gd != undefined){
            this.data.__loadParams.graphData = gd;
        }

        if(this.graphDataIndex < 0 || this.graphData[this.graphDataIndex] != tmp){
            this.graphDataIndex++;
            if(this.graphData.length > this.graphDataIndex){
                this.graphData.splice(this.graphDataIndex, this.graphData.length - this.graphDataIndex);
            }
            this.graphData.push(tmp);

            var limit = 200;
            if(WorkflowDesignerConstants.UndoDepth != undefined)
                limit = WorkflowDesignerConstants.UndoDepth;
            if(this.graphData.length > limit){
                this.graphData.splice(0, 1);
                this.graphDataIndex--;
            }
        }

        this.сheckToolbarButtonState();
    };

    this.ClearGraphData = function(){
        this.graphData = [];
        this.graphDataIndex = -1;
    };

    this.Undo = function(){
        if(this.graphDataIndex > 0){
            this.changeGraphDataIndex(this.graphDataIndex - 1);
        }
    };

    this.Redo = function(){
        if(this.graphData.length > this.graphDataIndex){
            this.changeGraphDataIndex(this.graphDataIndex + 1);
        }
    };

    this.changeGraphDataIndex = function(index){
        if(this.graphData.length > index){
            var data = JSON.parse(this.graphData[index]);
            WorkflowDesignerCommon.DataCorrection(data);
            this.designer.data = data;
            this.Draw(data);
            this.graphDataIndex = index;
        }
        
        this.сheckToolbarButtonState();
    };

    this.сheckToolbarButtonState = function(){
        if(this._toolbal == undefined)
            this._toolbal = me.GetComponentByType("WorkflowDesignerToolbar");
            
        var toolbar = this._toolbal;
        if(toolbar != undefined){
            toolbar.setItemDisabled('undo', this.graphDataIndex <= 0 );
            toolbar.setItemDisabled('redo', this.graphData.length <= (this.graphDataIndex + 1));
            toolbar.Layer.batchDraw();
        }
    };

    this.onSelectionChanged = function(predefinedSelection){
        if(this._toolbal == undefined)
            this._toolbal = me.GetComponentByType("WorkflowDesignerToolbar");
            
        var toolbar = this._toolbal;

        if(toolbar != undefined){
            var isSelected;
            if(predefinedSelection == undefined){
                var componentA = me.GetComponentByType("WorkflowDesignerActivityManager");
                var componentT = me.GetComponentByType("WorkflowDesignerTransitionManager");
                var selectedA = componentA.GetSelected();
                var selectedT = componentT.GetSelected();

                isSelected = selectedA.length > 0 || selectedT.length > 0;
            }
            else{
                isSelected = predefinedSelection;
            }
            
            toolbar.setItemDisabled('copy', !isSelected);
            toolbar.setItemDisabled('delete', !isSelected);
        }
    };
};