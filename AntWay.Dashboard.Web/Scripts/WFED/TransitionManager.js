function WorkflowDesignerTransitionManager() {
    this.type = 'WorkflowDesignerTransitionManager';

    this.init = function (graph) {
        this.graph = graph;
        this.Layer = new Konva.Layer();
        this.graph.Stage.add(this.Layer);
        this.Layer.setZIndex(2);

        this.graph = graph;
        this.APLayer = new Konva.Layer();
        this.graph.Stage.add(this.APLayer);
        this.APLayer.setZIndex(3);
    };
    this.ItemControls = new Array();
    this.draw = function () {
        if (this.ItemControls != null) {
            this.ItemControls.forEach(function (control) {
                control.destroy();
            });
        }

        this.ItemControls = new Array();

        var me = this;

        if (this.graph.data.Transitions != undefined) {
            this.graph.data.Transitions.forEach(function (item) {
                var cActivity = me.graph.GetComponentByType('WorkflowDesignerActivityManager');
                var itemControl = new WorkflowDesignerTransitionControl({
                    from: cActivity.find(item.From),
                    to: cActivity.find(item.To),
                    item: item,
                    graph: me.graph,
                    manager: me
                });

                me.ItemControls.push(itemControl);
                itemControl.Draw();
            });
        }

        this.batchDraw();
    };

    this.batchDraw = function () {

        this.CorrectItems();

        this.Layer.batchDraw();
        this.APLayer.batchDraw();
    };

    this.CorrectItems = function (){
        for(var i = 0; i < this.ItemControls.length; i++)
        {
            var item = this.ItemControls[i];
            for(var j=0; j < this.ItemControls.length; j++)
            {
                if (i == j) continue;

                var item2 = this.ItemControls[j];
                if (item.start.x == item2.start.x && item.start.y == item2.start.y)
                {
                    item2.start.x += 5;
                }

                if (item.end.x == item2.end.x && item.end.y == item2.end.y) {
                    item2.end.x += 5;
                }

                if (item.middle.x == item2.middle.x && item.middle.y == item2.middle.y) {
                    item2.middle.x += 15;
                }
            }
        }
    };

    this.getIntersectingActivity = function (point) {
        var cActivity = this.graph.GetComponentByType('WorkflowDesignerActivityManager');
        return cActivity.getIntersectingActivity(point);
    };
    this.LayerSetOffset = function (a) {
        this.Layer.setOffset(a);
        this.APLayer.setOffset(a);
    };
    this.LayerScale = function (a) {
        this.Layer.setScale({ x: this.Layer.getScale().x + a, y: this.Layer.getScale().y + a });
        this.APLayer.setScale({ x: this.APLayer.getScale().x + a, y: this.APLayer.getScale().y + a });
    };
    this.LayerScaleNorm = function () {
        this.Layer.setScale({ x: 1, y: 1 });
        this.Layer.setOffset({ x: 0, y: 0 });

        this.APLayer.setScale({ x: 1, y: 1 });
        this.APLayer.setOffset({ x: 0, y: 0 });
    };

    this.DeselectAll = function () {
        this.ItemControls.forEach(function (item) {
            item.Deselect();
        });
    };
    this.GetSelected = function () {
        var res = new Array();
        this.ItemControls.forEach(function (item) {
            if (item.selected) res.push(item);
        });
        return res;
    };


    this.SelectByPosition = function (rect) {
        this.ItemControls.forEach(function (item) {
            if (item.getIntersectingRect(rect)){
                item.Select();
            }
        });
    };
    this.SelectByItem = function (obj) {
        this.ItemControls.forEach(function (item) {
            if (item.item == obj)
                item.Select();
        });
    };

    this.CreateNewTransition = function (fromA, toA) {
        var me = this;

        if (toA == undefined) {
            var xs = fromA.control.getX() + fromA.rectangle.attrs.width;
            var ys = fromA.control.getY() + fromA.rectangle.attrs.height / 2;
            
            var pos = { x: xs, y: ys };
            var tt = new WorkflowDesignerTransitionManagerTempControl({ x: pos.x, y: pos.y, manager: this });
            tt.Draw(pos.x + 10, pos.y);
            this.batchDraw();

            var onMousemove = function (e) {
                var p = me.graph.CorrectPossition({ x: e.evt.offsetX, y: e.evt.offsetY }, me.Layer);
                tt.Redraw(p);
                me.Layer.batchDraw();
            };

            var onMouseup = function (e) {
                var pos = { x: e.evt.offsetX, y: e.evt.offsetY };
                var to = me.getIntersectingActivity(pos);
                if (to != undefined) {
                    me.CreateNewTransition(fromA, to);
                }
                tt.Delete();
                me.graph.Stage.off('mousemove.WorkflowDesignerTransitionManagerTempControl', onMousemove);
                me.graph.Stage.off('mouseup.WorkflowDesignerTransitionManagerTempControl', onMouseup);
                me.batchDraw();
                me.graph.StoreGraphData();
            };

            this.graph.Stage.on('mousemove.WorkflowDesignerTransitionManagerTempControl', onMousemove);
            this.graph.Stage.on('mouseup.WorkflowDesignerTransitionManagerTempControl', onMouseup);

            return tt;
        }
        else
        {
            var item = {
                Name: this.GetDefaultName(fromA.GetName(), toA.GetName()),
                From: fromA.item,
                To: toA.item,
                Trigger:    { Type: 'Auto'   },
                Conditions: [{ Type: 'Always' }],
                AllowConcatenationType: 'And',
                RestrictConcatenationType: 'And',
                ConditionsConcatenationType: 'And',
                Classifier: 'NotSpecified',
                DesignerSettings: {}
            };

            var itemControl = new WorkflowDesignerTransitionControl({
                from: fromA,
                to: toA,
                item: item,
                graph: me.graph,
                manager: me
            });

            me.ItemControls.push(itemControl);
            me.graph.data.Transitions.push(item);
            itemControl.Draw();
            
            return itemControl;
        }
    };

    this.GetDefaultName = function (a,b) {
        var name = a + '_' + b + '_';
        var index = 1;
        for (var i = 0; i < this.graph.data.Transitions.length; i++) {
            var item = this.graph.data.Transitions[i];
            if (item.Name == name + index) {
                index++;
                i = -1;
            }
        }
        return name + index;
    };
};