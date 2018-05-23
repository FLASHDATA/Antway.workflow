function WorkflowDesignerOverviewMap() {
    this.type = 'WorkflowDesignerOverviewMap';

    var me = this;

    this.init = function (graph) {
        this.graph = graph;
        this.Layer = new Konva.Layer();
        this.Layer.scale(0.5);
        this.graph.Stage.add(this.Layer);
        this.Layer.setZIndex(1000);

        var x = 20;
        var y = 12;

        var index = 0;
        
        var stageWidth = this.graph.Stage.width();
        var stageHeight = this.graph.Stage.height();

        var w = this.graph.Settings.overviewMapWidth != undefined ?
            this.graph.Settings.overviewMapWidth :
          WorkflowDesignerConstants.OverviewMap.width;

        var h = this.graph.Settings.overviewMapHeight != undefined ?
            this.graph.Settings.overviewMapHeight :
          WorkflowDesignerConstants.OverviewMap.height;

        me.background = new Konva.Image({
            x:  stageWidth - w - 5,
            y:  stageHeight - h - 5,
            width: w,
            height: h,
            fill: 'white',
            shadowEnabled: true,
            shadowBlur: 5,
            shadowOpacity: 0.3
        });

        me.Layer.add(me.background);
    };

    this.draw = function () {
        this.GraphRedrawAll();
    };

    this.GraphRedrawAll = function () {
        console.log('GraphRedrawAll');
        // var stageWidth = this.graph.Stage.width();
        // var stageHeight = this.graph.Stage.height();
        // var w = this.graph.Settings.overviewMapWidth != undefined ?
        //     this.graph.Settings.overviewMapWidth :
        //   WorkflowDesignerConstants.OverviewMap.width;

        // var h = this.graph.Settings.overviewMapHeight != undefined ?
        //     this.graph.Settings.overviewMapHeight :
        //   WorkflowDesignerConstants.OverviewMap.height;

        

        // me.background.x(stageWidth - w - 5);
        // me.background.y(stageHeight - h - 5);

        // var alayer = me.graph.GetComponentByType("WorkflowDesignerActivityManager").Layer;
        // alayer.toImage({callback:
        //     function (img){
        //         me.background.setImage(img);
        //     }
        // })
    };
};