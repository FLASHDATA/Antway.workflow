function WorkflowDesignerActivityControl(parameters) {
    var me = this;
    this.manager = parameters.manager;
    this.graph = parameters.graph;
    this.x = parameters.x;
    this.y = parameters.y;
    this.item = parameters.item;
    this.control = undefined;
    this.rectangle = undefined;
    this.text = undefined;
    this.createTransitionAndActivityButton = undefined;
    this.createTransitionButton = undefined;
    this.selected = false;

    this.dependentTransitions = new Array();
    this.getX = function () {
        return this.rectangle.attrs.x + this.control.attrs.x;
    };
    this.getY = function () {
        return this.rectangle.attrs.y + this.control.attrs.y;
    };

    this.GetName = function () {
        return this.item.Name;
    };

    this.SetName = function (v) {
        this.item.Name = v;
    };

    this.Draw = function () {
        var settings = me.graph.Settings;

        var draggable = !me.graph.Settings.disableobjectmovements;

        me.control = new Konva.Group({
            x: parameters.x,
            y: parameters.y,
            rotation: 0,
            draggable: draggable,
            dragBoundFunc: function (pos) {
                var kx = settings.DefaultMoveStep * me.manager.Layer.getScaleX();
                var ky = settings.DefaultMoveStep * me.manager.Layer.getScaleY();
                var pos = {
                    x: Math.round(pos.x / ky) * ky,
                    y: Math.round(pos.y / ky) * ky
                };

                if (me.selected) {
                    var oldpos = this.getAbsolutePosition();
                    me.manager.ObjectMove({ sender: me, changepos: { x: pos.x - oldpos.x, y: pos.y - oldpos.y } });
                }
                return pos;
            }
        });

        var rectColor = WorkflowDesignerConstants.ActivityColor;
        var textColor = WorkflowDesignerConstants.ActivityTextColor;
        var cornerRadius = 5;
        var width = undefined;
        var height = undefined;
        var xtext = 10;
        var rotation = 0;
        var xra = 0;
        var yra = 0;
        var offsetX = 0;
        var offsetY = 0;

        var isNormal = true;
        if (me.item.IsFinal) {
            rectColor = WorkflowDesignerConstants.ActivityFinalColor;
            textColor = WorkflowDesignerConstants.ActivityFinalTextColor;
            cornerRadius = 50;
            xtext = 20;
            isNormal = false;
        }

        if (me.item.IsInitial) {
            rectColor = WorkflowDesignerConstants.ActivityInitialColor;
            textColor = WorkflowDesignerConstants.ActivityInitialTextColor;
            cornerRadius = 50;
            xtext = 20;
            isNormal = false;
        }

        if (me.item.IsCondition) {
            rectColor = WorkflowDesignerConstants.ActivityConditionColor;
            textColor = WorkflowDesignerConstants.ActivityConditionTextColor;
            width = this.graph.Settings.DefaultActivityWidth;
            height = this.graph.Settings.DefaultActivityHeight;
            //cornerRadius = 50;
            xtext = 70;
            isNormal = false;
        }

        if (me.graph.GetCurrentActivity() == me.item.Name) {
            rectColor = WorkflowDesignerConstants.SelectColor;
            textColor = WorkflowDesignerConstants.SelectTextColor;
            width = this.graph.Settings.DefaultActivityWidth;
            height = this.graph.Settings.DefaultActivityHeight;
            cornerRadius = 50;
            isNormal = false;
        }

        if (me.graph.isCurrentActivityForSubprocess(me.item.Name)) {
            rectColor = WorkflowDesignerConstants.SelectSubProcessColor;
            textColor = WorkflowDesignerConstants.SelectSubProcessTextColor;
            cornerRadius = 50;
            isNormal = false;
        }
        //if (){
        // || 
        //}
        //var nose = me.item.Implementation;
        //alert(nose);


        //if (me.item.ImpAction == "InsertarNotificacionAction") {
        if (me.item.IsCondition) {
            me.rectangle = new Konva.Rect({
                x: xra,
                y: yra,
                offsetY: offsetY,
                offsetX: offsetX,
                width: this.graph.Settings.DefaultActivityWidth,
                height: this.graph.Settings.DefaultActivityHeight,
                // stroke: WorkflowDesignerConstants.ActivityShape,
                // strokeWidth: 0,
                rotation: rotation,
                fill: "#F4F4F4",
                cornerRadius: 5,
            });
            me.control.add(me.rectangle);
            me.impImage = new Konva.RegularPolygon({
                x: 100,
                y: 30,
                sides: 4,
                radius: 30,
                scaleX: 3.3,
                stroke: "#94948E",
                fill: "#FFE59E",
            });
            me.control.add(me.impImage);
        }
        else if (me.item.IsScheme) {
            me.rectangle = new Konva.Rect({
                x: xra,
                y: yra,
                offsetY: offsetY,
                offsetX: offsetX,
                width: this.graph.Settings.DefaultActivityWidth,
                height: this.graph.Settings.DefaultActivityHeight,
                // stroke: WorkflowDesignerConstants.ActivityShape,
                // strokeWidth: 0,
                rotation: rotation,
                fill: rectColor,
                cornerRadius: cornerRadius,
            });
            me.control.add(me.rectangle);

            me.impImage = new Konva.Image({
                x: me.rectangle.attrs.width - 40,
                y: 10,
                image: me.manager.ImageScheme,
                width: 25,
                height: 25
            });
            me.control.add(me.impImage);
        }
        else {
            me.rectangle = new Konva.Rect({
                x: xra,
                y: yra,
                offsetY: offsetY,
                offsetX: offsetX,
                width: this.graph.Settings.DefaultActivityWidth,
                height: this.graph.Settings.DefaultActivityHeight,
                // stroke: WorkflowDesignerConstants.ActivityShape,
                // strokeWidth: 0,
                rotation: rotation,
                fill: rectColor,
                cornerRadius: cornerRadius,
            });
            me.control.add(me.rectangle);

            if (Array.isArray(me.item.Implementation) && me.item.Implementation.length > 0) {
                me.impImage = new Konva.Image({
                    x: me.rectangle.attrs.width - 20,
                    y: 38,
                    image: isNormal ? me.manager.ImageImplementation : me.manager.ImageImplementationWhite,
                    width: 15,
                    height: 15
                });
                me.control.add(me.impImage);
            }
        }

        if (Array.isArray(me.item.PreExecutionImplementation) && me.item.PreExecutionImplementation.length > 0) {
            me.impImage2 = new Konva.Image({
                x: me.rectangle.attrs.width - 35,
                y: 38,
                image: isNormal ? me.manager.ImageImplementation : me.manager.ImageImplementationWhite,
                width: 15,
                height: 15
            });
            me.control.add(me.impImage2);
        }

        me.text = new Konva.Text({
            x: xtext,
            y: 10,
            text: this.GetName(),
            fontSize: 12,
            fontFamily: 'Arial',
            fontStyle: 'bold',
            fill: textColor
        });

        if (me.item.State == undefined)
            me.item.State = '';

        me.stateText = new Konva.Text({
            x: xtext,
            y: 25,
            text: me.item.State,
            fontSize: 12,
            fontFamily: 'Arial',
            fill: textColor
        });

        me.control.add(me.text);
        me.control.add(me.stateText);

        var typeText = "";
        if (me.item.IsInitial == true) {
            if (typeText.length > 0)
                typeText += " ";
            typeText += WorkflowDesignerConstants.ActivityFormLabel.IsInitial;
        }

        if (me.item.IsFinal == true) {
            if (typeText.length > 0)
                typeText += " ";
            typeText += WorkflowDesignerConstants.ActivityFormLabel.IsFinal;
        }

        if (me.item.IsCondition == true) {
            if (typeText.length > 0)
                typeText += " - ";
            typeText += WorkflowDesignerConstants.ActivityFormLabel.IsCondition;
        }

        if (typeText != "") {
            me.typeText = new Konva.Text({
                x: xtext,
                y: 40,
                text: typeText,
                fontSize: 12,
                fontFamily: 'Arial',
                fill: textColor
            });
            me.control.add(me.typeText);
        }

        if (me.graph.getParam("exinfo") == true) {
            me.createExInfo(me.control);
        }

        me.createIconNotify(me.control);

        if (!me.graph.Settings.disableobjectmovements) {
            this.control.on('dragend', this.Sync);
            this.control.on('dragmove', this._onMove);
            this.control.on('click', this._onClick);
            this.control.on('touchend', this._onClick);
        }
        this.control.on('dblclick', this._onDblClick);


        var path = me.graph.Settings.imagefolder;
        var buttons = [
            { img: path + 'wfe.settings.png', click: function () { me.ShowProperties(); } }];

        if (!me.graph.Settings.readonly) {
            buttons.push({ img: path + 'wfe.transition.png', click: function () { me._onCreateTransition(); } });
            buttons.push({ img: path + 'wfe.activity.png', click: function () { me._onCreateTransitionAndActivity(); } });
            buttons.push({ img: path + 'wfe.clone.png', click: function () { me.manager.Clone(me); } });
            buttons.push({ img: path + 'wfe.delete.png', click: function () { me._onDelete() } });
        }
        var pos = { x: me.rectangle.getWidth() - buttons.length * 30, y: -40 };
        me.bar = WorkflowDesignerBar(me.manager.Layer, buttons, pos);
        me.control.add(me.bar);
        me.bar.hide();

        me.manager.Layer.add(me.control);
    };
    this.Delete = function () {
        this.control.destroy();

        this.graph.data.Activities.splice(this.graph.data.Activities.indexOf(this.item), 1);
        this.manager.ItemControls.splice(this.manager.ItemControls.indexOf(this), 1);

        var todel = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            todel.push(this.dependentTransitions[i]);
        }

        for (var i = 0; i < todel.length; i++) {
            todel[i].Delete();
        }
    };
    this.Select = function () {
        if (this.selected == true)
            return;

        var me = this;
        this.rectangle.setStrokeWidth(4);
        this.rectangle.setStroke(WorkflowDesignerConstants.SelectColor);

        if (me.bar != unescape) {
            me.bar.show();
        }

        this.selected = true;
    };
    this.Deselect = function () {
        if (this.selected == false)
            return;

        this.rectangle.setStrokeWidth(0);
        this.rectangle.setStroke(this.rectangle.fill());

        if (me.bar != undefined)
            me.bar.hide();

        this.selected = false;
    };
    this.ObjectMove = function (e) {
        var pos = this.control.getAbsolutePosition();
        pos.x += e.x;
        pos.y += e.y;
        this.control.setAbsolutePosition(pos);

        this.Sync();

        if (me.dependentTransitions.length < 1)
            return;

        for (var i = 0; i < me.dependentTransitions.length; i++) {
            var t = me.dependentTransitions[i];
            t.middle = undefined;
            t.Draw();
        }
    };
    this._onMove = function () {
        if (me.dependentTransitions.length < 1)
            return;

        var delta = 20;
        var resetTransitionPoints = false;
        if (me.oldpos == undefined) {
            me.oldpos = me.control.getPosition();
        }
        else {
            var pos = me.control.getPosition();
            if (Math.abs(pos.x - me.oldpos.x) > delta || Math.abs(pos.y - me.oldpos.y) > delta)
                resetTransitionPoints = true;
        }

        for (var i = 0; i < me.dependentTransitions.length; i++) {
            var t = me.dependentTransitions[i];
            if (resetTransitionPoints) {
                t.middle = undefined;
                t.item.DesignerSettings = {};
            }
            t.Draw();
        }

        me.manager.redrawTransitions();
    };
    this._onClick = function (e) {
        var tmpSelect = me.selected;

        if (!e.evt.ctrlKey)
            me.graph.DeselectAll();

        if (tmpSelect)
            me.Deselect();
        else
            me.Select();

        me.graph.onSelectionChanged(!tmpSelect);
        me.manager.batchDraw();
    };
    this._onDblClick = function () {
        me.graph.DeselectAll();
        me.Select();
        me.manager.batchDraw();

        if (me.graph.Settings.notshowwindows)
            return;

        me.ShowProperties();
    };
    this._onDelete = function () {
        var me = this;
        me.graph.confirm(WorkflowDesignerConstants.DeleteConfirmCurrent, function () {
            me.Delete();
            me.graph.onSelectionChanged();
            me.graph.redrawAll();
            me.graph.StoreGraphData();
        });
    };
    this._onCreateTransitionAndActivity = function () {
        me.manager.createTransitionAndActivity(me);
        me.graph.StoreGraphData();
    };
    this._onCreateTransition = function () {
        me.manager.createTransition(me);
    };
    this.RegisterTransition = function (cTransition) {
        var f = false;

        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() == cTransition.GetName()) {
                f = true;
                break;
            }
        }

        if (!f)
            this.dependentTransitions.push(cTransition);
    };
    this.UnregisterTransition = function (cTransition) {
        var f = false;
        var nt = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() != cTransition.GetName()) {
                nt.push(this.dependentTransitions[i]);
            }
        }
        this.dependentTransitions = nt;
    };
    this.RegisterTransition = function (cTransition) {
        var f = false;

        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() == cTransition.GetName()) {
                f = true;
                break;
            }
        }

        if (!f)
            this.dependentTransitions.push(cTransition);
    };
    this.UnregisterTransition = function (cTransition) {
        var f = false;
        var nt = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() != cTransition.GetName()) {
                nt.push(this.dependentTransitions[i]);
            }
        }
        this.dependentTransitions = nt;
    };
    this.getRectPos = function () {
        var rectPos = this.rectangle.getAbsolutePosition();
        var xl = rectPos.x;
        var yl = rectPos.y;
        var xr = xl + this.rectangle.getWidth() * this.manager.Layer.getScaleX();
        var yr = yl + this.rectangle.getHeight() * this.manager.Layer.getScaleY();
        return { xl: xl, yl: yl, xr: xr, yr: yr };
    };
    this.getIntersectingActivity = function (point) {
        var pos = this.getRectPos();
        return point.x >= pos.xl && point.x < pos.xr && point.y >= pos.yl && point.y < pos.yr;
    };
    this.getIntersectingActivityRect = function (a) {
        var b = this.getRectPos();
        if (a.xl > b.xr || a.xr < b.xl || a.yl > b.yr || a.yr < b.yl)
            return false;
        return true;
    };

    this.ShowProperties = function () {
        var labels = WorkflowDesignerConstants.ActivityFormLabel;

        var impparam = [
            { name: labels.ImpOrder, code: 'impOrder', field: "Order", type: "input", width: "40px" },
            {
                name: labels.ImpAction,
                code: 'impAction',
                field: "ActionName",
                type: "select",
                datasource: me.graph.getActionNames()
            },
            {
                name: labels.ImpActionParameter,
                code: 'impparam',
                field: "ActionParameter",
                type: "json",
                openautocompleteonclick: true,
                datasource: function (request, response) {
                    var tr = $(this.element[0]).closest("tr");
                    var actionName = tr.find("[name=impAction]")[0].value;
                    response(me.graph.getAutoCompleteSuggestions("actionparameter", actionName, request.term));
                }
            }
        ];

        var params = {
            type: 'form',
            title: labels.Title,
            width: '800px',
            data: this.item,
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.State, field: "State", type: "input" },
                //{ name: "Field1", field: "Field1", type: "input" },
                //{ name: "Field2", field: "Field2", type: "input" },
                {
                    type: 'group', elements: [
                        { name: labels.IsInitial, field: "IsInitial", type: "checkbox" },
                        { name: labels.IsFinal, field: "IsFinal", type: "checkbox" },
                        { name: labels.IsCondition, field: "IsCondition", type: "checkbox" },
                        //{ name: labels.IsAutoSchemeUpdate, field: "IsAutoSchemeUpdate", type: "checkbox" },
                        { name: labels.IsScheme, field: "IsScheme", type: "checkbox" }
                    ]
                },
                {
                    name: labels.Implementation, field: "Implementation", type: "table", elements: impparam,
                    onrowadded: function (row) {
                        var order = row.find('[name=impOrder]');
                        if (order[0].value === "")
                            order[0].value = row.parent().children().length;
                    }
                },
                {
                    name: labels.PreExecutionImplementation, field: "PreExecutionImplementation", type: "table", elements: impparam,
                    onrowadded: function (row) {
                        var order = row.find('[name=impOrder]');
                        if (order[0].value === "")
                            order[0].value = row.parent().children().length;
                    }
                }],
            graph: me.graph,
            readonly: me.graph.Settings.readonly
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;
            isValid &= formControl.CheckRequired([data], ['Name'], WorkflowDesignerConstants.FieldIsRequired);

            me.graph.data.Activities.forEach(function (a) {
                if (a != me.item && a.Name == data.Name) {
                    isValid = false;
                    formControl.ControlAddError(data.control_Name, WorkflowDesignerConstants.FieldMustBeUnique);
                }
            });

            if (!formControl.CheckRequired(data.Implementation, ['ActionName', 'Order'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }

            if (!formControl.CheckRequired(data.PreExecutionImplementation, ['ActionName', 'Order'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }
            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);

                me.item.Name = data.Name;
                me.item.State = data.State;
                me.item.IsInitial = data.IsInitial;
                me.item.IsFinal = data.IsFinal;
                me.item.IsForSetState = false;
                me.item.IsAutoSchemeUpdate = true; // data.IsAutoSchemeUpdate;
                me.item.IsScheme = data.IsScheme;
                me.item.IsCondition = data.IsCondition;

                me.item.Implementation = data.Implementation;
                me.item.PreExecutionImplementation = data.PreExecutionImplementation;

                WorkflowDesignerCommon.DataCorrection(me.graph.data);
                me.graph.Draw(me.graph.data);
                me.graph.StoreGraphData();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.Sync = function () {
        if (me.item.DesignerSettings == undefined)
            me.item.DesignerSettings = {};

        var pos = me.control.getPosition();

        me.item.DesignerSettings.X = pos.x;
        me.item.DesignerSettings.Y = pos.y;

        me.oldpos = undefined;
    };

    this.createIconNotify = function (obj) {

        var textNotify = "";
        if (Array.isArray(me.item.Implementation) && me.item.Implementation.length > 0) {
            me.item.Implementation.forEach(function (item) {
                if (textNotify.length > 0)
                    textNotify += ", ";

                textNotify += item.ActionName;
            });
        }

        if (textNotify.includes("InsertarNotificacionAction")) {
            var Notify = new Konva.Text({
                x: 170,
                y: this.graph.Settings.DefaultActivityHeight - 50,
                text: "🔔",
                fontFamily: 'Arial',
                fontSize: 20,
                fill: '#4A4A4A',
                fontStyle: 'bold'
            });
            
            obj.add(Notify);
        }
    };

    this.createExInfo = function (obj) {
        var tooltiptext = "";

        if (Array.isArray(me.item.Implementation) && me.item.Implementation.length > 0) {
            me.item.Implementation.forEach(function (item) {
                if (tooltiptext.length > 0)
                    tooltiptext += ", ";

                tooltiptext += item.ActionName;
            });
        }

        var tooltiptext2 = "";
        if (Array.isArray(me.item.PreExecutionImplementation) && me.item.PreExecutionImplementation.length > 0) {
            me.item.PreExecutionImplementation.forEach(function (item) {
                if (tooltiptext2.length > 0)
                    tooltiptext2 += ", ";
                tooltiptext2 += item.ActionName;
            });

            if (tooltiptext == "")
                tooltiptext = WorkflowDesignerConstants.None;
        }

        if (tooltiptext.length > 0) {
            var textctrl = new Konva.Text({
                x: 10,
                y: this.graph.Settings.DefaultActivityHeight + 5,
                text: tooltiptext,
                fontFamily: 'Arial',
                fontSize: 12,
                fill: '#4A4A4A',
                fontStyle: 'bold'
            });

            obj.add(textctrl);
        }


        if (tooltiptext2.length > 0) {
            var textctrl2 = new Konva.Text({
                x: 10,
                y: this.graph.Settings.DefaultActivityHeight + textctrl.getHeight() + 5,
                text: tooltiptext2,
                fontFamily: 'Arial',
                fontSize: 12,
                fill: '#4A4A4A',
                fontStyle: 'italic'
            });

            obj.add(textctrl2);
        }

    };

    this.destroy = function () {
        this.control.destroy();
    }
};