function WorkflowDesignerTooltip(layer, obj, text, shift, deltaX) {
    obj.destroyToolTip = function() {
        if (this.ToolTip != undefined) {
            this.ToolTip.destroy();
            this.ToolTip = undefined;
            layer.batchDraw();
        }
    }

    var correctPossition = function (e, layer) {
        if (layer.getScaleX() == 0 || layer.getScaleY() == 0)
            return { x: layer.getOffsetX(), y: 0 };

        var dx = deltaX == undefined ? 0 : deltaX;
        return {
            x: e.x / layer.getScaleX() + layer.getOffsetX() + dx,
            y: e.y / layer.getScaleY() + layer.getOffsetY()
        }
    };

    obj.on('mouseover',
        function() {
            if (obj.ToolTip != undefined) {
                var pos = correctPossition(obj.getAbsolutePosition(), layer);
                obj.ToolTip.position({
                    x: pos.x + obj.getWidth() / 2,
                    y: pos.y + shift,
                    opacity: 1
                });
                obj.ToolTip.show();
            }
            else{
                var pos = correctPossition(obj.getAbsolutePosition(), layer);

                var tooltip = new Konva.Label({
                    x: pos.x + obj.getWidth() / 2,
                    y: pos.y + shift,
                    opacity: 1
                });

                tooltip.add(new Konva.Tag({
                    fill: '#3D4D59',
                    pointerDirection: 'up',
                    pointerWidth: 13,
                    pointerHeight: 7,
                    lineJoin: 'round',
                    shadowColor: '#3D4D59',
                    shadowBlur: 10,
                    shadowOffset: 10,
                    shadowOpacity: 0,
                    cornerRadius: 5
                }));

                tooltip.add(new Konva.Text({
                    text: text,
                    fontFamily: 'Arial',
                    fontSize: 12,
                    padding: 5,
                    fill: 'white'
                }));

                layer.add(tooltip);
                
                obj.ToolTip = tooltip;
            }
            layer.batchDraw();
        });

    obj.on('mouseleave',
        function() {
            if(obj.ToolTip != undefined){
                obj.ToolTip.hide();
                layer.batchDraw();
            } 
        });
}