function WorkflowDesignerBackground() {

    this.type = 'WorkflowDesignerBackground';
    this.init = function (graph) {
        var me = this;
        
        this.graph = graph;
        this.BackgroundLayer = new Konva.Layer();
        this.graph.Stage.add(this.BackgroundLayer);
        this.BackgroundLayer.setZIndex(0);

        this.SelectionLayer = new Konva.Layer();
        this.graph.Stage.add(this.SelectionLayer);
        this.SelectionLayer.setZIndex(1);


        WorkflowDesignerCommon.loadImage(this.graph.Settings.imagefolder + 'wfe.grid.png', function(image){
            me.RectBG.setFillPatternImage(image);
            me.BackgroundLayer.batchDraw();
        });
        
        this.RectBG = new Konva.Rect({
            x: 0,
            y: 0,
            width: 5000,
            height: 5000,
            draggable: false,
            dragBoundFunc: function (pos) {
                var kx = me.graph.Settings.DefaultMoveStep * me.BackgroundLayer.getScaleX();
                var ky = me.graph.Settings.DefaultMoveStep * me.BackgroundLayer.getScaleY();
                var correctpos = {
                    x: Math.round(pos.x / kx) * kx,
                    y: Math.round(pos.y / ky) * ky
                };

                me.graph.GraphLayerSetOffset(-correctpos.x / me.BackgroundLayer.getScaleX(), -correctpos.y / me.BackgroundLayer.getScaleY());
                return correctpos;
            },
            designerparam: 'background'
        });

        this.BackgroundLayer.add(this.RectBG);
        this.BackgroundLayer.batchDraw();
        this.graph.Stage.on('mousedown.background', function (evt) {
            if (evt.target.attrs.designerparam === 'background') {
                if(me._movemodeenabled != true)
                    me._mousedownpos = me.graph.CorrectPossition({ x: evt.evt.offsetX, y: evt.evt.offsetY }, me.SelectionLayer);
                else{
                    me.graph.DeselectAll();
                }
            }
        });

        this.graph.Stage.on('mousemove.background', function (evt) {
            if (me._movemodeenabled != true && me._mousedownpos != undefined) {
                var pos = me.graph.CorrectPossition({ x: evt.evt.offsetX, y: evt.evt.offsetY }, me.SelectionLayer);
                me.DrawSelectionRect(pos);
            }
        });

        this.graph.Stage.on('mouseup.background', function (e) {
            if (!me._movemodeenabled && me._mousedownpos != undefined) {
                var pos = me.getSelectionRectPos();
                if (pos == undefined) {
                    if(!e.evt.ctrlKey){
                       me.graph.DeselectAll();
                    }
                }
                else if (Math.abs(pos.xl - pos.xr) > 10 || Math.abs(pos.yl - pos.yr) > 10) {
                    me.graph.DeselectAll();
                    me.graph.ComponentsExecute('SelectByPosition', pos);
                    me.graph.onSelectionChanged();
                }
            }
            me._mousedownpos = undefined;
            me.DeleteSelectionRect();
        });
    };

    this.setMoveModeEnabled = function (flag) {
        this._movemodeenabled = Boolean(flag);
        this.RectBG.setDraggable(this._movemodeenabled);
        this.graph.setParam("movemodeenabled", this._movemodeenabled);
    };

    this.setPosition = function(pos){
        this.RectBG.setPosition({x: -pos.x, y: -pos.y});
        this.graph.ComponentsExecute('LayerSetOffset', { 
            x: pos.x, y: pos.y
         });
    };

    this.LayerScale = function (a) {
        this.BackgroundLayer.setScale({ x: this.BackgroundLayer.getScale().x + a, y: this.BackgroundLayer.getScale().y + a });
    }

    this.LayerScaleNorm = function () {
        this.BackgroundLayer.setScale({ x: 1, y: 1 });
        this.SelectionLayer.setScale({ x: 1, y: 1 });
        this.RectBG.setPosition({ x: 0, y: 0 });
    }

    this.DrawSelectionRect = function (pos) {
        if (this.RectSelection == undefined) {
            this.RectSelection = new Konva.Rect({
                x: this._mousedownpos.x,
                y: this._mousedownpos.y,
                width: pos.x - this._mousedownpos.x,
                height: pos.y - this._mousedownpos.y,
                draggable: false,
                fill: '#66CCFF',
                opacity: 0.2
            });
            this.SelectionLayer.add(this.RectSelection);
        }
        else {

            this.RectSelection.setWidth(pos.x - this._mousedownpos.x);
            this.RectSelection.setHeight(pos.y - this._mousedownpos.y);
        }

        this.SelectionLayer.batchDraw();
    };

    this.DeleteSelectionRect = function (pos) {
        if (this.RectSelection) {
            this.RectSelection.destroy();
            this.RectSelection = undefined;
            this.SelectionLayer.batchDraw();
        }
    };

    this.getSelectionRectPos = function () {
        if (this.RectSelection == undefined)
            return undefined;
        var rectPos = this.RectSelection.getAbsolutePosition();
        var xl = rectPos.x;
        var yl = rectPos.y;
        var xr = xl + this.RectSelection.getWidth() * this.SelectionLayer.getScaleX();
        var yr = yl + this.RectSelection.getHeight() * this.SelectionLayer.getScaleX();
        return { xl: Math.min(xl, xr), yl: Math.min(yl, yr), xr: Math.max(xl, xr), yr: Math.max(yl, yr) };
    };
};