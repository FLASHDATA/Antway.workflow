function WorkflowDesignerBar(layer, buttons, pos, direct) {
    var vertical = direct == 'v';

    var block = new Konva.Group({
        x: pos.x,
        y: pos.y
    });

    var bg = new Konva.Rect({
        x: 0,
        y: 0,
        width: vertical ? 30 : (buttons.length * 30),
        height: vertical ? (buttons.length * 30) : 30,
        fill: WorkflowDesignerConstants.BarColor,
        cornerRadius: 5
    });
   
    block.add(bg);

    var offsetX = 5;
    var offsetY = 5;
    var offset = 0;
    buttons.forEach(function (item) {
        item.offset = offset;
        
        if (item.separator == true){
            offset += 10;
            var points = vertical ? 
                [offsetX, item.offset + offsetY, offsetX + 20, item.offset + offsetY] :
                [item.offset + offsetX, offsetY, item.offset + offsetX, 20 + offsetY];
            item.cObject = new Konva.Line({
                points: points,
                stroke: WorkflowDesignerConstants.BarSeparatorColor,
                strokeWidth: 2
            });
            block.add(item.cObject);
        }
        else{
            offset += 30;

            WorkflowDesignerCommon.loadImage(item.img, function(image){
                    var pos = vertical ? 
                        {x: offsetX, y: item.offset + offsetY} :
                        {x: offsetX + item.offset, y:  offsetY};
                    [item.offset + offsetX, offsetY, item.offset + offsetX, 20 + offsetY];
    
                    item.group = new Konva.Group({
                        x: pos.x - offsetX,
                        y: pos.y - offsetY,
                        width: 30,
                        height: 30
                    });

                    item.bg = new Konva.Rect({
                        x: 0,
                        y: 0,
                        width: 30,
                        height: 30,
                        cornerRadius: 5
                     });
    
                    if(item.active == true)
                        item.bg.setFill(WorkflowDesignerConstants.ButtonActive);

                    item.group.add(item.bg);
    
                    item.cImageToolbar = new Konva.Image({
                        x: offsetX,
                        y: offsetY,
                        image: image,
                        width: 20,
                        height: 20,
                        strokeWidth: 0
                    });
                    item.group.add(item.cImageToolbar);
    
                    var onclick = function(){
                        if(item.cImageToolbar.ToolTip != undefined)
                            item.cImageToolbar.ToolTip.hide();
                            
                        if(item.click != undefined)
                            item.click();

                    };
                    item.group.on('click', onclick);
                    item.group.on('touchend', onclick);
                    item.group.on('mouseover',
                        function() {
                            item.bg.setFill(WorkflowDesignerConstants.ButtonActive);
                            layer.batchDraw();
                    });
                    item.group.on('mouseleave',
                        function() {
                            if(item.active == undefined || item.active == false){
                                item.bg.setFill('');
                                layer.batchDraw();
                            }
                    });

                    item.group.add(item.cImageToolbar);
    
                    block.add(item.group);
                    if(item.title != undefined && item.title != "")
                        WorkflowDesignerTooltip(layer, item.cImageToolbar, item.title, 30);
    
                    layer.batchDraw();
            });
        }
    });

    if(vertical == true)
        bg.setHeight(offset);
    else
        bg.setWidth(offset);

    return block;
}