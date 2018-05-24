function WorkflowDesignerToolbar() {
    this.type = 'WorkflowDesignerToolbar';

    var me = this;

    this.init = function (graph) {
        this.graph = graph;
        this.Layer = new Konva.Layer();
        this.graph.Stage.add(this.Layer);
        this.Layer.setZIndex(10);
        this.InitItems();

        me.background = new Konva.Rect({
            x: 0,
            y: 0,
            width: this.graph.Settings.graphwidth,
            height: 50,
            fill: WorkflowDesignerConstants.BarColor,
            shadowEnabled: true,
            shadowBlur: 5,
            shadowOpacity: 0.3
        });
        me.Layer.add(me.background);

        var maintoolbarX = 0;
        var maintoolbarY = 15;
        this.MainToolbarDraw(maintoolbarX, maintoolbarY);
        me.SideToolbar = WorkflowDesignerBar(me.Layer,
            me.SideItems, { x: me.graph.Settings.graphwidth - 60, y: 70}, 'v');
        me.Layer.add(me.SideToolbar);

        this.CreateInfoBlock({
            x: this.graph.Settings.graphwidth - 100,
            y: maintoolbarY - 13
        });

        var bgComponent = this.GetWorkflowDesignerBackground();
        if(bgComponent != undefined)
            bgComponent.RectBG.setDraggable(false);
        
        if(this.graph.getParam("movemodeenabled") == true){
            me.ToolbarMovePress();
        }

        if(this.graph.getParam("exinfo") == true){
           me.ToolbarExInfoPress();
        }   
    };

    this.MainToolbarDraw = function (x, y){
        var me = this;
        var itemOffset = 0;
        this.Items.forEach(function (item) {
            item.offset = itemOffset;
            
            if (item.separator == true){
                itemOffset += 1;
                item.cObject = new Konva.Line({
                    points: [x + item.offset, 0, 
                        x + item.offset, 50],
                    stroke: WorkflowDesignerConstants.BarSeparatorColor,
                    strokeWidth: 2
                });
                me.Layer.add(item.cObject);
            }
            else{
                itemOffset += 50;
                WorkflowDesignerCommon.loadImage(item.img, function(image){
                    item.group = new Konva.Group({
                        x: x + item.offset,
                        y: y - 15,
                        width: 50,
                        height: 50,
                    });

                    item.bg = new Konva.Rect({
                        x: 0,
                        y: 0,
                        width: 50,
                        height: 50,
                        cornerRadius: 0
                     });
                     item.group.add(item.bg);
                    
                    if(item.active == true)
                        item.bg.setFill(WorkflowDesignerConstants.ButtonActive);

                    item.cImageToolbar = new Konva.Image({
                        x: 15,
                        y: 15,
                        image: image,
                        width: 20,
                        height: 20,
                        strokeWidth: 0
                    });

                    if(item.disabled == true){
                        item.cImageToolbar.opacity(0.3);
                    }
                    item.group.add(item.cImageToolbar);

                    item.group.on('click', function(){ 
                        if(item.disabled !== true && item.click != undefined)
                            item.click(item);
                    });
                    item.group.on('touchend', function(){ 
                        if(item.disabled !== true && item.click != undefined)
                            item.click(item);
                    });
                    
                    item.group.on('mouseover',
                    function() {
                        if(item.disabled != true){
                            item.bg.setFill(WorkflowDesignerConstants.ButtonActive);
                            me.Layer.batchDraw();
                        }

                    });
                    item.group.on('mouseleave',
                        function() {
                            if(item.active == undefined || item.active == false){
                                item.bg.setFill('');
                                me.Layer.batchDraw();
                            }
                    });
                    me.Layer.add(item.group);
                    WorkflowDesignerTooltip(me.Layer, item.group, item.title, 55, item.offset == 0 ? 20 : 0);
                    me.Layer.batchDraw();
                });
            }
        });
    };

    this.draw = function () {
        this.GraphRedrawAll();
    };

    this.GraphRedrawAll = function () {
        this.UpdateInfoBlock();
        this.Layer.batchDraw();
    };

    this.changeWidth = function(width){
        this.background.width(width);
        this.info.position({x: width - 100, y: 2});
        this.SideToolbar.position({x: width - 60, y: 70})
    };

    this.CreateInfoBlock = function (pos) {
        this.infoText = new Konva.Text({
            text: this.GetInfoBlockText(),
            fontFamily: 'Arial',
            fontSize: 12,
            padding: 5,
            fill: 'black'
        });

        this.infoTextValue = new Konva.Text({
            text: this.GetInfoBlockTextValue(),
            fontFamily: 'Arial',
            fontSize: 12,
            padding: 5,
            fill: 'black',
            x: 60
        });

        this.info = new Konva.Group(pos);
        this.info.add(this.infoText);
        this.info.add(this.infoTextValue);
        this.Layer.add(this.info);
    }

    this.UpdateInfoBlock = function (pos) {
        this.infoTextValue.setText(this.GetInfoBlockTextValue());
    };

    this.GetInfoBlockText = function () {
        return WorkflowDesignerConstants.InfoBlockLabel.Activity + '\r\n' +
            WorkflowDesignerConstants.InfoBlockLabel.Transition + '\r\n' +
            WorkflowDesignerConstants.InfoBlockLabel.Command;
    };

    this.GetInfoBlockTextValue = function () {
        var aCount = 0;
        var tCount = 0;
        var commandCount = 0;

        if (this.graph.data != undefined) {
            aCount = this.graph.data.Activities.length;
            tCount = this.graph.data.Transitions.length;
            commandCount = this.graph.data.Commands.length;
        }

        var values = [aCount, tCount, commandCount];
        var res = '';
        values.forEach(function (n) {
            var separ = '';
            var str = n.toString();
            for (var i = 0; i < 4 - str.length; i++)
                separ += '  ';

            if (res.length > 0)
                res += '\r\n';
            res += separ + str;
        });
        
        return res;
    };

    this.ToolbarMovePress = function () {
        var bgComponent = this.GetWorkflowDesignerBackground();
        bgComponent.setMoveModeEnabled(!bgComponent._movemodeenabled);
        var obj = this.GetItemByCode('move');
        if (bgComponent._movemodeenabled) {
            obj.active = true;
            if(obj.bg != undefined)
                obj.bg.setFill(WorkflowDesignerConstants.ButtonActive);
        }
        else {
            obj.active = undefined;
            if(obj.bg != undefined)
                obj.bg.setFill('');
        }
        this.Layer.batchDraw();
    };

    this.ToolbarExInfoPress = function () {
        var obj = this.GetItemByCode('exinfo');
        if (!this.exinfo) {
            obj.active = true;
            if(obj.bg != undefined)
                obj.bg.setFill(WorkflowDesignerConstants.ButtonActive);
            
            this.exinfo = true;
            this.graph.setParam("exinfo", true);
        }
        else {
            obj.active = undefined;
            if(obj.bg != undefined)
                obj.bg.setFill('');

                this.exinfo = false;
            this.graph.setParam("exinfo", false);
        }

        if(this.graph.Draw != undefined)
            this.graph.Draw(this.graph.data);
    };

    this.GetWorkflowDesignerBackground = function () {
        return this.graph.GetComponentByType("WorkflowDesignerBackground");
    };

    this.CreateActivity = function () {
        var component = this.graph.GetComponentByType("WorkflowDesignerActivityManager");
        component.CreateNewActivity();
        this.graph.redrawAll();
        this.graph.StoreGraphData();
    };

    this.AutoArrangement = function () {
        if (me.graph.data.Activities.length == 0)
            return;

        var activityStarts = new Array();
        me.graph.data.Activities.forEach(function (item) {
            if(item.IsInitial){
                activityStarts.push(item);
            }
        });
     
        me.graph.data.Activities.forEach(function (item) {
            if (!item.IsInitial) {
                var isFind = true;
                for (var i = 0; i < me.graph.data.Transitions.length; i++) {
                    var trans = me.graph.data.Transitions[i];
                    if (trans.Classifier == 'Direct' && trans.To == item) {
                        isFind = false;
                        break;
                    }
                }

                if (isFind)
                    activityStarts.push(item);
            }
        });

        if (activityStarts.length == 0)
            activityStarts.push(me.graph.data.Activities[0]);

        var pos = { x: 80, y: 120 };
        var step = { x: 300, y: 140 };

        var processedActivity = Array();
        
        var arrangementActivityFunc = function (items, startPos, isFirst) {        
            var currpos = { x: startPos.x, y: startPos.y };

            if(!isFirst)
                currpos.x += step.x;

            var tmpArray = new Array();
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item.DesignerSettings == undefined)
                    item.DesignerSettings = {};

                if (!isFirst && $.inArray(item, processedActivity) >= 0) {
                    continue;
                }

                item.DesignerSettings.X = currpos.x;
                processedActivity.push(item);
                tmpArray.push(item);
            }

            for (var i = 0; i < tmpArray.length; i++) {
                var item = tmpArray[i];
              
                if (i > 0)
                    currpos.y += step.y;

                var activityChildren = new Array();
                me.graph.data.Transitions.forEach(function (trans) {
                    if (trans.Classifier == 'Direct' && trans.From == item){
                        activityChildren.push(trans.To);
                    }
                });

                item.DesignerSettings.Y = currpos.y;
                var endPos = arrangementActivityFunc(activityChildren, { x: currpos.x, y: currpos.y });
                currpos.y = endPos.y;
            }
            return { x: currpos.x, y: currpos.y };
        };
        
        arrangementActivityFunc(activityStarts, pos, true);

        me.graph.data.Transitions.forEach(function (t1) {
            if (t1.DesignerSettings == undefined)
                t1.DesignerSettings = {};
            t1.DesignerSettings.X = undefined;
            t1.DesignerSettings.Y = undefined;
        });

        me.graph.Draw(me.graph.data);
        this.graph.StoreGraphData();
    };
    
    this.CopySelectedGenUniqueValue = function (value, array, property) {
        var name = value;
        for (var i = 1; true; i++)
        {
            var isFind = false;
            for(var j = 0; j < array.length; j++){
                if (array[j][property] == name) {
                    isFind = true;
                    break;
                }
            }

            if (!isFind) {
                break;
            }            
            name = value + '_' + i;
        }

        return name;
    }

    this.CopySelected = function () {
        var componentA = this.graph.GetComponentByType("WorkflowDesignerActivityManager");
        var componentT = this.graph.GetComponentByType("WorkflowDesignerTransitionManager");

        var selectedA = componentA.GetSelected();
        var selectedT = componentT.GetSelected();

        if(selectedA.length == 0 && selectedT.length == 0){
            return;
        }

        var newObjectsA = [];
        selectedA.forEach(function (item) {
            var newItem = JSON.parse(JSON.stringify(item.item));
            newItem.DesignerSettings.Y += 160;
            newItem.Name = me.CopySelectedGenUniqueValue(newItem.Name, me.graph.data.Activities, 'Name');

            newObjectsA.push({
                oldItem: item.item,
                newItem: newItem
            });

            me.graph.data.Activities.push(newItem);
        });

        var newObjectsT = [];
        selectedT.forEach(function (item) {
            var aFrom = item.item.From;
            var aTo = item.item.To;
            for (var i = 0; i < newObjectsA.length; i++) {
                if (aFrom == newObjectsA[i].oldItem)
                    aFrom = newObjectsA[i].newItem;

                if (aTo == newObjectsA[i].oldItem)
                    aTo = newObjectsA[i].newItem;
            }
            
            var newItem = JSON.parse(JSON.stringify(item.item));
            newItem.Name = me.CopySelectedGenUniqueValue(newItem.Name, me.graph.data.Transitions, 'Name');
            newItem.From = aFrom;
            newItem.To = aTo;

            newObjectsT.push({
                oldItem: item.item,
                newItem: newItem
            });

            me.graph.data.Transitions.push(newItem);
        });

        WorkflowDesignerCommon.DataCorrection(me.graph.data);
        me.graph.Draw(me.graph.data);

        newObjectsA.forEach(function (item) {
            componentA.SelectByItem(item.newItem);
        });

        newObjectsT.forEach(function (item) {
            componentT.SelectByItem(item.newItem);
        });

        this.graph.StoreGraphData();
        this.graph.onSelectionChanged(true);
    };

    this.EditLocalization = function () {
        var labels = WorkflowDesignerConstants.LocalizationFormLabel;
        
        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.graph.data.Localization,
            datadefault: { Culture: WorkflowDesignerConstants.DefaultCulture, Type : 'State' },
            elements: [
                { name: labels.ObjectName, field: "ObjectName", type: "input" },
                {
                    name: labels.Type, field: "Type", type: "select", displayfield: 'Name', valuefield: 'Value',
                    datasource: [{ Name: labels.Types[0], Value: 'Command' },
                                    { Name: labels.Types[1], Value: 'State' },
                                    { Name: labels.Types[2], Value: 'Parameter' }]
                },
                { name: labels.IsDefault, field: "IsDefault", type: "checkbox" },
                { name: labels.Culture, field: "Culture", type: "input" },
                { name: labels.Value, field: "Value", type: "input" }
            ],
            readonly : this.graph.Settings.readonly
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['ObjectName', 'Type', 'Culture', 'Value'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['ObjectName', 'Type', 'Culture'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.graph.data.Localization, data, params);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditTimer = function () {
        var labels = WorkflowDesignerConstants.TimerFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.graph.data.Timers,
            keyproperty: 'Name',
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "select", datasource: this.graph.data.AdditionalParams.TimerTypes },
                { name: labels.Value, field: "Value", type: "input" },
                { name: labels.NotOverrideIfExists, field: "NotOverrideIfExists", type: "checkbox" }
            ],
            readonly : this.graph.Settings.readonly
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Type', 'Value'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);

                if (me.graph.data.Timers == undefined)
                    me.graph.data.Timers = [];

                me.SyncTable(me.graph.data.Timers, data, params);
                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditActors = function () {
        var me = this;
        var labels = WorkflowDesignerConstants.ActorFormLabel;
        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.graph.data.Actors,
            keyproperty: 'Name',
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Rule, field: "Rule", type: "select", datasource: this.graph.getActorNames() },
                {
                    name: labels.Value, field: "Value", type: "json", openautocompleteonclick:true, datasource: function (request,response) {
                        var tr = $(this.element[0]).closest("tr");
                        var ruleName = tr.find("[name=Rule]")[0].value;
                        response(me.graph.getAutoCompleteSuggestions("ruleparameter", ruleName, request.term));
                    }
                }
            ],
            graph: me.graph,
            readonly : this.graph.Settings.readonly
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Rule'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.graph.data.Actors, data, params);
                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditParameters = function() {
        var labels = WorkflowDesignerConstants.ParameterFormLabel;

        var me = this;

        var getemptytype = function (form, control, callback)
        {
            var typename = undefined;
            var parameterName = undefined;
            var items = form.getEditData(form.parameters);
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                
                if (item.control_InitialValue.id === control.id){
                        typename = item.Type;
                        break;
                }
            }
                                        
            if (typename != undefined)
            {
                 me.graph.designer.getemptytype(encodeURIComponent(typename),callback);
            }                   
        };

        var params = {
            type: 'table',
            title: labels.Title,
            data: this.graph.data.Parameters,
            datadefault: { Purpose: 'Persistence' },
            keyproperty: "Name",
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "input", datasource: this.graph.getTypeNames(), width: "35%" },
                { name: labels.Purpose, field: "Purpose", type: "select", displayfield: 'Name', datasource: [{ Name: 'Temporary' }, { Name: 'Persistence' }, { Name: 'System' }] },
                { name: labels.InitialValue, field: "InitialValue", type: "json", width: "25%", getemptytype : getemptytype }
            ],
            top: $('<div style="float: right; margin-bottom: 15px;"></div>'),
            beforerowadded: function(item) {
                if (item.Type != undefined)
                    item.Type = decodeURIComponent(item.Type);
            },
            onrowadded: function(row) {
                var sPurpose = row.find('[name=Purpose]');
                var purpose = sPurpose[0];
                sPurpose.change(function() {
                        setFieldsReadonly(row);
                    });
                
                if (purpose != undefined && purpose.value !== "System") {
                    row.find('[name=Purpose] option[value="System"]').remove();
                }

                setFieldsReadonly(row);
            },
            graph : me.graph,
            readonly : this.graph.Settings.readonly
        };

        var hideSystem = function() {
            var table = $(form.window.find('.WorkflowDesignerTable'))[0];
            for (var i = 0; i < table.rows.length; i++) {
                var row = $(table.rows[i]);
                var purpose = row.find('[name=Purpose]')[0];
                if (purpose != undefined && purpose.value === "System")
                    row.hide();
            }
        }

        var showSystem = function() {
            var table = $(form.window.find('.WorkflowDesignerTable'))[0];
            for (var i = 0; i < table.rows.length; i++) {
                var row = $(table.rows[i]);
                var purpose = row.find('[name=Purpose]')[0];
                if (purpose != undefined && purpose.value === "System")
                    row.show();
            }
        }

        var setFieldsReadonly = function(row) {
            var purpose = row.find('[name=Purpose]')[0];
                if (purpose != undefined && purpose.value === "System") {
                    row.find(':input').attr('readonly', true);
                    row.find('[name=Purpose]').attr('disabled', true);
                    row.find('[name=InitialValue]').val('');
                    row.find('.btnDelete').remove();
                } 
                else if (purpose != undefined && purpose.value === "Temporary") {
                    row.find('[name=InitialValue]').attr('readonly', true);
                    row.find('[name=InitialValue]').val('');
                } 
                else if (purpose != undefined && purpose.value === "Persistence") {
                    row.find('[name=InitialValue]').attr('readonly', false);
                } 
        }


        var setSystemFieldsReadonly = function() {
            var table = $(form.window.find('.WorkflowDesignerTable'))[0];
            for (var i = 0; i < table.rows.length; i++) {
                var row = $(table.rows[i]);
                setFieldsReadonly(row);
            }
        }
       
        var saveFunc = function(data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Type', 'Purpose', 'Parameter'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.graph.data.Parameters, data, params);
                for (var i = 0; i < me.graph.data.Parameters.length; i++) {
                    var type = me.graph.data.Parameters[i].Type;
                    me.graph.data.Parameters[i].Type = encodeURIComponent(type);
                }

                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        var form = new WorkflowDesignerForm(params);

        var showsystemparams = $('<div class="ui slider checkbox"></div>');
        var cbShowsystemparams = $('<input type="checkbox" />');
        cbShowsystemparams.click(function(e, s) {
            var checked = cbShowsystemparams[0].checked;
            if (checked) {
                showSystem();
            } else {
                hideSystem();
            }
        
            WorkflowDesignerCommon.modal(form.window, "refresh");
        });
        showsystemparams.append(cbShowsystemparams);
        showsystemparams.append('<label>'+WorkflowDesignerConstants.ParameterFormLabel.ShowSystemParameters+'</label>');
        params.top.append(showsystemparams);

        form.showModal(saveFunc);
        hideSystem();
        setSystemFieldsReadonly();
        WorkflowDesignerCommon.modal(form.window, "refresh");
    };

    this.EditCodeActions = function () {
        var labels = WorkflowDesignerConstants.CodeActionsFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            data: this.graph.data.CodeActions,
            datadefault: {},
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "select", displayfield: 'Name', datasource: [{ Name: 'Action' }, { Name: 'Condition' }, { Name: 'RuleGet' }, { Name: 'RuleCheck' }] },
                { name: labels.IsGlobal, field: "IsGlobal", type: "checkbox" }
            ],
            graph : me.graph,
            readonly: this.graph.Settings.readonly,
            onrowadded: function (row, f) {
                var sType = row.find('[name=Type]');
                var type = sType[0];
                sType.change(function () {
                    setAsyncReadonly(row);
                });

                setAsyncReadonly(row);

                var sGlobal = row.find('[name=IsGlobal]');
                sGlobal.change(function(){
                    if(this.checked == false){
                        f.InfoDialog(WorkflowDesignerConstants.Warning, WorkflowDesignerConstants.CodeActionsFormLabel.UnGlobalMessage, "mini");
                    }
                });
            },
            onrowdelete: function (row, f) {
                var sGlobal = row.find('[name=IsGlobal]');
                if(sGlobal[0].checked)
                    f.InfoDialog(WorkflowDesignerConstants.Warning, WorkflowDesignerConstants.CodeActionsFormLabel.GlobalDeleteMessage, "mini");
            }
        };

        if (!WorkflowDesignerConstants.isjava) {
            params.elements.push({ name: labels.IsAsync, field: "IsAsync", type: "checkbox" });
        }

        params.elements.push({ name: labels.ActionCode, field: "ActionCode", type: "code" });

        var form = new WorkflowDesignerForm(params);

        var setAsyncReadonly = function(row) {
            if (WorkflowDesignerConstants.isjava)
                return;
            var type = row.find('[name=Type]')[0];
            var asyncCheckBox = row.find('[name=IsAsync]');
            if (type !== undefined && (type.value === "RuleGet" || type.value === "RuleCheck")) {
                asyncCheckBox.attr('disabled', true);
                asyncCheckBox.attr('checked', false);
            } else {
                asyncCheckBox.attr('disabled', false);
            }
        };

        var saveFunc = function(data, formparams) {
            for (var j = 0; j < data.length; j++)
            {
                var dataItem = data[j];
                if (dataItem.ActionCode !== undefined && (dataItem.ActionCode.code === undefined || !dataItem.ActionCode.code))
                {
                    if (dataItem.Type === "Action")
                    {
                        dataItem.ActionCode.code = "return;"
                    }
                    else if (dataItem.Type === "Condition")
                    {
                        dataItem.ActionCode.code = "return false;"
                    }
                    else if (dataItem.Type === "RuleGet")
                    {
                        dataItem.ActionCode.code = "return new List<string>();"
                    }
                    else if (dataItem.Type === "RuleCheck")
                    {
                        dataItem.ActionCode.code = "return false;"
                    }
                }
            }
            
            if (form.CheckRequired(data, ['Name', 'Type', 'ActionCode.code'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name', 'Type', 'IsGlobal'], WorkflowDesignerConstants.FieldMustBeUnique)) {
               
                form.ClearTempField(data);
                
                var names = me.getGlobalCodeActionsForDelete(me.graph.data.CodeActions, data);
                me.SyncTable(me.graph.data.CodeActions, data, params);
                for (var i = 0; i < me.graph.data.CodeActions.length; i++) {
                    var code = me.graph.data.CodeActions[i].ActionCode;
                    me.graph.data.CodeActions[i].ActionCode = encodeURIComponent(code.code);
                    if (!WorkflowDesignerConstants.isjava)
                        me.graph.data.CodeActions[i].Usings = encodeURIComponent(code.usings);
                }
                if(names.length > 0){
                    me.graph.designer.deleteGlobalCodeAction(names, function (response) {
                        if(response.isError == true){
                            form.InfoDialog(WorkflowDesignerConstants.EditCodeLabel.Error, response.errorMessage);
                        }
                        
                        return false;
                    });
                }
                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.getGlobalCodeActionsForDelete = function(original, target){
        var names = [];
        original.forEach(function(ca){
            if(ca.IsGlobal){
                var isSameName = false;
                for(var i=0; i < original.length; i++){
                    if(ca.Name == original[i].Name && ca.IsGlobal != original[i].IsGlobal){
                        isSameName = true;
                        break;
                    }
                }

                var isFind = false;
                for(var i=0; i < target.length; i++){
                    if(isSameName == true){
                        if(ca.Name == target[i].Name && target[i].IsGlobal){
                            isFind = true;
                            break;
                        }
                    }
                    else{
                        if(ca.Name == target[i].Name){
                            isFind = true;
                            break;
                        }
                    }
                }

                if(isFind == false)
                    names.push(ca.Name);
            }
        });
        return names;
    };

    this.EditCommands = function () {
        var labels = WorkflowDesignerConstants.CommandFormLabel;
        var me = this;

        var getemptytype = function (form, control, callback)
        {
            var typename = undefined;
            var parameterName = undefined;
            var items = form.getEditData(form.parameters);
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item.InputParameters == undefined)
                    continue;
                for (var j = 0; j < item.InputParameters.length; j++)
                {
                    if (item.InputParameters[j].control_DefaultValue.id === control.id){
                        parameterName = item.InputParameters[j].Parameter.Name;
                        break;
                    }
                }   
            }
                                        
            if (parameterName != undefined)
            {
                var allParameters = me.graph.data.Parameters;
                for (var i = 0; i < allParameters.length; i++)
                {
                    if (allParameters[i].Name === parameterName)
                    {
                        typename = allParameters[i].Type;
                        break;
                    }
                }
            }

            if (typename != undefined)
            {
                 me.graph.graph.getemptytype(typename,callback);
            }
                                   
        };

        var params = {
            type: 'table',
            title: labels.Title,
            width: '900px',
            data: this.graph.data.Commands,
            datadefault: {},
            keyproperty: 'Name',
            elements: [
                { name: labels.Name , field: "Name", type: "input" },
                {
                    name: labels.InputParameters, field: "InputParameters", type: "table", elements: [
                        { name: labels.InputParametersName, code: 'ipname', field: "Name", type: "input" },
                        { name: labels.InputParametersParameter, code: 'ipparameter', field: "Parameter.Name", type: "select", displayfield: 'Name', datasource: this.graph.getNonSystemParameters() },
                        { name: labels.InputParametersIsRequired, code: 'iisrequired', field: "IsRequired", type: "checkbox" },
                        { name: labels.InputParametersDefaultValue, code: 'idefaultvalue', field: "DefaultValue", type: "json", width:'40%', getemptytype: getemptytype}
                    ]
                }
            ],
            graph: me.graph,
            readonly : this.graph.Settings.readonly
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;

            isValid &= formControl.CheckRequired(data, ['Name'], WorkflowDesignerConstants.FieldIsRequired);
            isValid &= formControl.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique);
            data.forEach(function (item) {
                if(!formControl.CheckRequired(item.InputParameters, ['Name', 'Parameter.Name'], WorkflowDesignerConstants.FieldIsRequired)){
                    isValid = false;
                }
            });
            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);
                me.SyncTable(me.graph.data.Commands, data, params);
                WorkflowDesignerCommon.DataCorrection(me.graph.data);
                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditAdditionalParameters = function () {
        var labels = WorkflowDesignerConstants.AdditionalParamsFormLabel;

        var params = {
            type: 'form',
            title: labels.Title,
            width: '800px',
            data: this.graph.data.AdditionalParams,
            readonly: true, 
            elements: [
                { name: labels.IsObsolete, field: "IsObsolete", type: "checkbox" },
                { name: labels.DefiningParameters, field: "DefiningParameters", type: "textarea" },
                {
                    name: labels.ProcessParameters , field: "ProcessParameters", type: "table", elements: [
                        { name: labels.ProcessParametersName, field: "Name", type: "input" },
                        { name: labels.ProcessParametersValue, field: "Value", type: "input" }
                    ]
                }
            ]
        };
        
        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            return true;          
        };

        form.showModal(saveFunc);
    };

    this.Items = [];
    this.SideItems = [];

    this.InitItems = function () {
        var path = this.graph.Settings.imagefolder;
        
        if (!this.graph.Settings.readonly){
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.CreateActivity, img: path + 'wfe.add.png', click: function () { me.CreateActivity(); } });
            this.Items.push({ code: 'copy', disabled: true, title: WorkflowDesignerConstants.ToolbarLabel.CopySelected, img: path + 'wfe.clone.png', click: function () { me.CopySelected(); } });
            this.Items.push({ code: 'delete', disabled: true, title: WorkflowDesignerConstants.ToolbarLabel.Delete, img: path + 'wfe.delete.png', click: function () { me.graph.DeleteSelected(); } });
            this.Items.push({ separator: true });
            this.Items.push({ code: 'undo', title: WorkflowDesignerConstants.ToolbarLabel.Undo, img: path + 'wfe.undo.png', click: function () { me.graph.Undo(); } });
            this.Items.push({ code: 'redo', title: WorkflowDesignerConstants.ToolbarLabel.Redo, img: path + 'wfe.redo.png', click: function () { me.graph.Redo(); } });
            this.Items.push({separator: true});
        }

        if (!this.graph.Settings.notshowwindows){
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Actors, img: path + 'wfe.actors.png', click: function () { me.EditActors(); } });
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Commands, img: path + 'wfe.commands.png', click: function () { me.EditCommands(); } });
            this.Items.push( { title: WorkflowDesignerConstants.ToolbarLabel.Timers, img: path + 'wfe.timers.png', click: function () { me.EditTimer(); } });
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.CodeActions, img: path + 'wfe.codeactions.png', click: function () { me.EditCodeActions(); } });
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Parameters, img: path + 'wfe.parameters.png', click: function () { me.EditParameters(); } });
            this.Items.push({separator: true});
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.AdditionalParameters, img: path + 'wfe.context.png', click: function () { me.EditAdditionalParameters(); } });
            this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Localization, img: path + 'wfe.localization.png', click: function () { me.EditLocalization(); } });
            this.Items.push({separator: true});
        }

        if (!this.graph.Settings.disableobjectmovements){
            this.Items.push( {  title: WorkflowDesignerConstants.ToolbarLabel.AutoArrangement, img: path + 'wfe.autoarrangment.png', click: function () { me.AutoArrangement(); } });
            this.Items.push({separator: true});
        }
        this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Refresh, img: path + 'wfe.refresh.png', click: function () { me.graph.Refresh(); } });
        this.Items.push({ code: 'exinfo', title: WorkflowDesignerConstants.ToolbarLabel.Info, img: path + 'wfe.information.png', click: function () { me.ToolbarExInfoPress(); } });
        this.Items.push({ title: WorkflowDesignerConstants.ToolbarLabel.Legend, img: path + 'wfe.help.png', click: function () { me.ShowLegend(); } });
        
        this.SideItems = [
            { title: WorkflowDesignerConstants.ToolbarLabel.ZoomIn, img: path + 'wfe.zoomin.png', click: function () { me.graph.GraphLayerScale(0.1); } },
            { title: WorkflowDesignerConstants.ToolbarLabel.ZoomOut, img: path + 'wfe.zoomout.png', click: function () { me.graph.GraphLayerScale(-0.1); } },
            { title: WorkflowDesignerConstants.ToolbarLabel.ZoomPositionDefault, img: path + 'wfe.defaultzoom.png', click: function () { me.graph.GraphLayerScaleNorm(); } },
            {separator: true},
            { code: 'move', title: WorkflowDesignerConstants.ToolbarLabel.Move, img: path + 'wfe.move.png', click: function () { me.ToolbarMovePress(); } },
            { code: 'fullscreen', title: WorkflowDesignerConstants.ToolbarLabel.FullScreen, img: path + 'wfe.fullscreen.png', click: function () { me.graph.onFullScreenClick(); } }
        ]; 
        
        me.graph.Stage.getContent().addEventListener('wheel', function (e) {
            var bg = me.GetWorkflowDesignerBackground();
            var oldScale = bg.BackgroundLayer.scaleX();

            var mousePointTo = {
                x: e.offsetX / oldScale - bg.RectBG.x(),
                y: e.offsetY / oldScale - bg.RectBG.y()
            };

            var scaleBy = 0.1;
            var deltaScale = e.deltaY > 0 ? -scaleBy : scaleBy;
            var newScale = oldScale + deltaScale;

            var newPos = {
                x: mousePointTo.x - mousePointTo.x / newScale,
                y: mousePointTo.y - mousePointTo.y / newScale
            };

            var correctPos= {
                x: ((1 - oldScale) * e.offsetX / oldScale - bg.RectBG.x()) / newScale,
                y: ((1 - oldScale) * e.offsetY / oldScale - bg.RectBG.y()) / newScale,
            } 
            newPos.x += correctPos.x;
            newPos.y += correctPos.y;
            
            me.graph.ComponentsExecute("LayerScale", deltaScale);
            me.graph.ComponentsExecute('setPosition', newPos);
            me.graph.redrawAll();
        });
    };
    
    this.GetItemByCode = function (code) {
        for (var i = 0; i < this.Items.length; i++) {
            var a = this.Items[i];
            if (a.code == code)
                return a;
        }

        for (var i = 0; i < this.SideItems.length; i++) {
            var a = this.SideItems[i];
            if (a.code == code)
                return a;
        }
        return undefined;
    };

    this.SyncTable = function (source, dest, params) {
        if (params.keyproperty == undefined) {
            source.splice(0, source.length);
            for (var i = 0; i < dest.length; i++) {
                var newItem = {};
                params.elements.forEach(function (e) {
                    newItem[e.field] = dest[i][e.field];
                });
                source.push(newItem);
            }
        }
        else {
            for (var i = source.length - 1; i >= 0; i--) {
                var findEl = $.grep(dest, function (el) {
                    return source[i][params.keyproperty] == el.keyproperty;
                });

                if (findEl.length == 0)
                    source.splice(i, 1);
                else {
                    params.elements.forEach(function (e) {
                        source[i][e.field] = findEl[0][e.field];
                    });
                }
            }

            for (var i = 0; i < dest.length; i++) {
                var findEl = $.grep(source, function (el) {
                    return dest[i][params.keyproperty] == el[params.keyproperty];
                });

                if (findEl.length == 0) {
                    var newItem = {};
                    params.elements.forEach(function (e) {
                        newItem[e.field] = dest[i][e.field];
                    });
                    source.push(newItem);
                }
            }
        }
    };

    this.ShowLegend = function(){
        var path = this.graph.Settings.imagefolder;
        var img = $('<image src="' + path + 'wfe.legend.png" height="'+ this.graph.Stage.getHeight() * 0.7 +'"/>');
        var w = $('<div class="ui modal"></div>')
                    .append($('<div class="content" style="text-align: center;"></div>')
                        .append(img));
        
        WorkflowDesignerCommon.modal(w, "show");
    };

    this.setItemDisabled = function(code, flag){
        for(var i=0; i < this.Items.length; i++){
            if(this.Items[i].code == code){
                this.Items[i].disabled = flag;
                if(this.Items[i].cImageToolbar != undefined){
                    this.Items[i].cImageToolbar.opacity(flag == true ? 0.3 : 1);
                }
                break;
            }
        }

        for(var i=0; i < this.SideItems.length; i++){
            if(this.SideItems[i].code == code){
                this.SideItems[i].disabled = flag;
                if(this.Items[i].cImageToolbar != undefined){
                    this.Items[i].cImageToolbar.opacity(flag == true ? 0.3 : 1);
                }
                break;
            }
        }
    };

    this.setItemActive = function(code, flag){
        for(var i=0; i < this.Items.length; i++){
            if(this.Items[i].code == code){
                this.Items[i].active = flag;
                if(this.Items[i].bg != undefined){
                    this.Items[i].bg.setFill(flag == true ? WorkflowDesignerConstants.ButtonActive : '');
                }
                break;
            }
        }

        for(var i=0; i < this.SideItems.length; i++){
            if(this.SideItems[i].code == code){
                this.SideItems[i].active = flag;
                if(this.SideItems[i].bg != undefined){
                    this.SideItems[i].bg.setFill(flag == true ? WorkflowDesignerConstants.ButtonActive : '');
                }
                break;
            }
        }
    };
};