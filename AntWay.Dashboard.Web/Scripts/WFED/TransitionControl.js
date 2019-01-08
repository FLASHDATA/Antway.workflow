function WorkflowDesignerTransitionControl(parameters) {
    var me = this;
    this.manager = parameters.manager;
    this.graph = parameters.graph;
    this.from = parameters.from;
    this.to = parameters.to;
    this.item = parameters.item;

    this.setFrom = function (activity) {
        this.from = activity;
        this.item.From = activity.item;
    };

    this.setTo = function (activity) {
        this.to = activity;
        this.item.To = activity.item;
    };

    this.GetName = function () {
        return this.item.Name;
    };

    this.SetName = function (v) {
        this.item.Name = v;
    };

    this.control = undefined;
    this.arrow = undefined;
    this.line = undefined;

    if (this.item.DesignerSettings != undefined && this.item.DesignerSettings.X != undefined && this.item.DesignerSettings.Y != undefined) {
        this.middle = { x: Number(this.item.DesignerSettings.X), y: Number(this.item.DesignerSettings.Y) };
    }
    else
        this.middle = undefined;

    this.from.RegisterTransition(this);
    this.to.RegisterTransition(this);

    this.start = undefined;
    this.end = undefined;

    this.angle = undefined;

    this.activePoint = undefined;
    this.touchpoints = [];

    this.DrawTransition = function (fixedstartpoit, fixedendpoint) {

        var me = this;
        var fromRec = this.from.rectangle;
        var toRec = this.to.rectangle;

        var fromx, fromy, tox, toy;
        fromx = Number(this.from.getX());
        fromy = Number(this.from.getY());
        tox = Number(this.to.getX());
        toy = Number(this.to.getY());

        var w2 = Number(fromRec.attrs.width / 2); var h2 = Number(fromRec.attrs.height / 2);
        var dsize = 25;

        var ascx = fromx + w2;
        var ascy = fromy + h2;
        var aecx = tox + w2;
        var aecy = toy + h2;

        this.direction = { start: 0, end: 0 };

        if (this.from == this.to) {
            this.start = { x: ascx + w2, y: ascy - h2 + 14 };
            this.end = { x: aecx + w2 - 25, y: aecy - h2 };
            this.direction.end = 1;
            if (this.middle == undefined)
                this.middle = { x: ascx + w2 + h2, y: ascy - 2 * h2 };
        }
        else {
            var recalcMiddle = false;
            if (this.middle == undefined) {
                recalcMiddle = true;
                this.middle = { x: (ascx + aecx) / 2, y: (ascy + aecy) / 2 };
            }
            var xs = ascx, ys = ascy, xe = aecx, ye = aecy;


            if (ascy - h2 - dsize > this.middle.y && aecy - h2 - dsize > this.middle.y) {
                ys = ascy - h2;
                ye = aecy - h2;
                this.direction.start = 1;
                this.direction.end = 1;
            }
            else if (ascy + h2 + dsize < this.middle.y && aecy + h2 + dsize < this.middle.y) {
                ys = ascy + h2;
                ye = aecy + h2;
                this.direction.start = 1;
                this.direction.end = 1;
            }
            else if (ascx - w2 - dsize > this.middle.x && aecx - w2 - dsize > this.middle.x) {
                xs = ascx - w2;
                xe = aecx - w2;
            }
            else
                if (ascx + w2 + dsize < this.middle.x && aecx + w2 + dsize < this.middle.x) {
                    xs = ascx + w2;
                    xe = aecx + w2;
                }
                else {
                    //calculate start point

                    if (ascx + w2 + dsize < this.middle.x) xs += w2;
                    else if (ascx - w2 - dsize > this.middle.x) xs -= w2;
                    else if (ascy + h2 + dsize < this.middle.y) {
                        ys += h2;
                        this.direction.start = 1;
                    }
                    else if (ascy - h2 - dsize > this.middle.y) {
                        ys -= h2;
                        this.direction.start = 1;
                    }
                    else {
                        if (xs <= this.middle.x)
                            xs += w2;
                        else {
                            xs -= w2;
                            //this.direction.start = 1;
                        }
                    }

                    //calculate end point
                    if (aecx + w2 + dsize < this.middle.x) xe += w2;
                    else if (aecx - w2 - dsize > this.middle.x) xe -= w2;
                    else if (aecy + h2 + dsize < this.middle.y) {
                        ye += h2;
                        this.direction.end = 1;
                    }
                    else if (aecy - h2 - dsize > this.middle.y) {
                        ye -= h2;
                        this.direction.end = 1;
                    }
                    else {
                        if (ys >= this.middle.y)
                            ye += h2;
                        else {
                            ye -= h2;
                        }
                        this.direction.end = 1;
                    }
                }

            if (fixedstartpoit != undefined) {
                xs = fixedstartpoit.x;
                ys = fixedstartpoit.y;
                recalcMiddle = true;
            }


            if (fixedendpoint != undefined) {
                xe = fixedendpoint.x;
                ye = fixedendpoint.y;
                recalcMiddle = true;
            }

            this.start = { x: xs, y: ys };
            this.end = { x: xe, y: ye };

            if (recalcMiddle) {
                this.middle = { x: (xs + xe) / 2, y: (ys + ye) / 2 };

                for (var i = 0; i < me.manager.ItemControls.length; i++) {
                    var item = me.manager.ItemControls[i];
                    if (item == me) continue;

                    if (item.middle.x == me.middle.x && item.middle.y == me.middle.y) {
                        if (me.direction.start == 0)
                            me.middle.y += 40;
                        else
                            me.middle.x += 40;
                    }
                }
            }

            //Correct start/end points
            if (fixedstartpoit == undefined) {
                if (this.direction.start == 0) {
                    if (this.middle.y > ys + h2 - 7)
                        ys += h2 - 7;
                    else if (this.middle.y < ys - h2 + 7)
                        ys -= h2 - 7;
                    else
                        ys = this.middle.y;
                }
                else {
                    if (this.middle.x > xs + w2 - 10)
                        xs += w2 - 10;
                    else if (this.middle.x < xs - w2 + 10)
                        xs -= w2 - 10;
                    else
                        xs = this.middle.x;
                }
            }

            if (fixedendpoint == undefined) {
                if (this.direction.end == 0) {
                    if (this.middle.y > ye + h2 - 7)
                        ye += h2 - 7;
                    else if (this.middle.y < ye - h2 + 7)
                        ye -= h2 - 7;
                    else
                        ye = this.middle.y;
                }
                else {
                    if (this.middle.x > xe + w2 - 10)
                        xe += w2 - 10;
                    else if (this.middle.x < xe - w2 + 10)
                        xe -= w2 - 10;
                    else
                        xe = this.middle.x;
                }
            }

            this.start = { x: xs, y: ys };
            this.end = { x: xe, y: ye };
        }

        var color = this.GetColor();
        this.points = this.GetPoints([this.start.x, this.start.y, this.middle.x, this.middle.y, this.end.x, this.end.y], this.direction);

        this.angle = Math.atan2(
            this.points[this.points.length - 1] - this.points[this.points.length - 3],
            this.points[this.points.length - 2] - this.points[this.points.length - 4]);

        if (this.control == undefined) {
            this.control = new Konva.Group({
                x: 0,
                y: 0,
                rotation: 0
            });

            this.arrow = WorkflowDesignerCommon.createArrowByAngle(this.end.x, this.end.y, this.angle, 15, color);
            var lineConfig = {
                points: this.points,
                stroke: color,
                strokeWidth: 2,
                lineCap: 'round',
                lineJoin: 'round'
            };
            if (this.item.IsFork)
                lineConfig.dash = [10, 10];

            this.line = new Konva.Line(lineConfig);
            this.control.add(this.line);
            this.control.add(this.arrow);
            this.manager.Layer.add(this.control);
        }
        else {
            WorkflowDesignerCommon.updateArrowByAngle(this.arrow, this.end.x, this.end.y, this.angle, 15, color);
            this.line.setPoints(this.points);
        }
    };

    this.GetPoints = function (points, direction) {
        // one line
        if (points[0] == points[2] == points[4] || points[1] == points[3] == points[5])
            return points;

        var res = new Array();

        if (points[0] == points[2] || points[1] == points[3]) {
            res.push(points[0], points[1], points[2], points[3]);
        }
        else {
            res.push(points[0], points[1]);

            if (direction.start == 0)
                res.push(points[2], points[1]);
            else
                res.push(points[0], points[3]);

            res.push(points[2], points[3]);
        }

        if (direction.end == 0 && res[res.length - 2] != points[4]) {
            res.push(res[res.length - 2], points[5]);
        }
        else if (direction.end == 1 && res[res.length - 1] != points[5]) {
            res.push(points[4], res[res.length - 1]);
        }
        res.push(points[4], points[5]);

        return res;
    };

    this.GetColor = function () {
        var classifier = this.item.Classifier == undefined
                            ? 'notspecified'
            : this.item.Classifier.toLowerCase();

        //console.log(classifier);
        if (classifier == 'TimeInterval') return '#3F8C8D';

        return classifier == 'notspecified'
            ? '#7F8C8D'
            : (classifier == 'direct') ? '#27AE60' : '#2980B9';
    };

    this.DrawActivePoint = function () {
        if (this.activePoint) {
            this._moveActivePoint(this.middle.x, this.middle.y);
        }
        else {
            var activePoint = this._createActivePoint(this.middle.x, this.middle.y, this.control);
            me.manager.APLayer.add(activePoint);
            this.activePoint = activePoint;
        }
    };

    this.DrawTouchPoints = function () {
        var tpShift = this._getLineLength(this.start.x, this.start.y, this.end.x, this.end.y) * 0.1;

        if (this.touchpoints[0] != undefined && !this.touchpoints[0].isdestroyed) {
            this._moveTouchPoints(this.touchpoints[0], this.points, false);
        } else {
            var touchPoint1 = this._createTouchPoint(this.points, this.control, false);
            me.manager.APLayer.add(touchPoint1);
            this.touchpoints[0] = touchPoint1;
        }

        if (this.touchpoints[1] != undefined && !this.touchpoints[1].isdestroyed) {
            this._moveTouchPoints(this.touchpoints[1], this.points, true);
        } else {
            var touchPoint2 = this._createTouchPoint(this.points, this.control, true);
            me.manager.APLayer.add(touchPoint2);
            this.touchpoints[1] = touchPoint2;
        }
    };

    this.Draw = function (fixedstartpoit, fixedendpoint) {
        this.DrawTransition(fixedstartpoit, fixedendpoint);
        this.DrawActivePoint();
        if (!this.graph.Settings.readonly) {
            this.DrawTouchPoints();
        }
    };

    this.DeleteTouchPoint = function (isend) {
        for (var i = 0; i < this.touchpoints.length; i++) {
            if (this.touchpoints[i].isend === isend) {
                this.touchpoints[i].destroy();
                this.touchpoints[i].isdestroyed = true;
            }
        }
    };

    this.Delete = function () {
        this.from.UnregisterTransition(this);
        this.to.UnregisterTransition(this);
        this.control.destroy();

        if (this.activePoint.ToolTip != undefined) {
            this.activePoint.ToolTip.destroy();
        }
        this.activePoint.destroy();
        for (var i = 0; i < this.touchpoints.length; i++) {
            this.touchpoints[i].destroy();
        }

        var index = this.graph.data.Transitions.indexOf(this.item);
        if (index >= 0)
            this.graph.data.Transitions.splice(index, 1);

        index = this.manager.ItemControls.indexOf(this);
        if (index >= 0)
            this.manager.ItemControls.splice(index, 1);
    };

    this._onDelete = function () {
        me.graph.confirm(WorkflowDesignerConstants.DeleteConfirmCurrent, function () {
            me.Delete();
            me.graph.onSelectionChanged();
            me.graph.redrawAll();
            me.graph.StoreGraphData();

        });
    };

    this.Select = function () {
        if (this.selected == true)
            return;

        var me = this;
        me.oldstroke = this.line.getStroke();
        me.line.setStroke(WorkflowDesignerConstants.SelectColor);
        me.line.setStrokeWidth(3);


        if (me.bar == undefined) {
            var path = me.graph.Settings.imagefolder;
            var barX = -15;
            var buttons = [{ img: path + 'wfe.settings.png', click: function () { me.ShowProperties() } }];
            if (!me.graph.Settings.readonly) {
                buttons.push({ img: path + 'wfe.delete.png', click: function () { me._onDelete(); } });
                barX = -30;
            }
            me.bar = WorkflowDesignerBar(me.manager.APLayer, buttons, { x: barX, y: -50 });
            me.activePoint.add(me.bar);
        }
        else {
            this.bar.show();
        }

        me.selected = true;
    };

    this.Deselect = function () {
        if (this.selected == false)
            return;

        this.line.setStrokeWidth(2);
        if (this.oldstroke != undefined) {
            this.line.setStroke(this.oldstroke);
        }

        if (this.bar != undefined)
            this.bar.hide();

        this.selected = false;
    };

    this._moveTouchPoints = function (tp, points, isend) {
        var point = isend ?
            { x: points[points.length - 2], y: points[points.length - 1] } :
            { x: points[0], y: points[1] };

        var point2 = isend ?
            { x: points[points.length - 4], y: points[points.length - 3] } :
            { x: points[2], y: points[3] };

        var offset = { x: 0, y: 0 };
        var offsetValue = isend ? 24 : 10;
        if (point.x == point2.x) {
            if (point.y < point2.y)
                offset.y = offsetValue;
            else
                offset.y = -offsetValue;
        }
        else if (point.y == point2.y) {
            if (point.x < point2.x)
                offset.x = offsetValue;
            else
                offset.x = -offsetValue;
        }

        tp.setPosition(point);
        tp.circle.setPosition(offset);
    };

    this._createTouchPoint = function (points, cTransition, isend) {
        var me = this;

        var point = isend ?
            { x: points[points.length - 2], y: points[points.length - 1] } :
            { x: points[0], y: points[1] };

        var point2 = isend ?
            { x: points[points.length - 4], y: points[points.length - 3] } :
            { x: points[2], y: points[3] };

        var offset = { x: 0, y: 0 };
        var offsetValue = isend ? 24 : 10;
        if (point.x == point2.x) {
            if (point.y < point2.y)
                offset.y = offsetValue;
            else
                offset.y = -offsetValue;
        }
        else if (point.y == point2.y) {
            if (point.x < point2.x)
                offset.x = offsetValue;
            else
                offset.x = -offsetValue;
        }

        var cTouchPoint = new Konva.Group({
            x: point.x,
            y: point.y,
            draggable: true
        });

        cTouchPoint.isend = isend;
        var color = this.GetColor();
        var circle = new Konva.Circle({
            x: offset.x,
            y: offset.y, //////////////////////////////////////
            radius: 5,
            fill: color
        });

        cTouchPoint.add(circle);
        cTouchPoint.circle = circle;
        cTouchPoint.transition = cTransition;

        var redraw = function () {
            var position = me.graph.CorrectPossition(circle.getAbsolutePosition(), me.manager.Layer);

            if (isend) {
                me.DrawTransition(undefined, position);
            } else {
                me.DrawTransition(position, undefined);
            }
            me.DrawActivePoint();
            me.DeleteTouchPoint(!isend);
            me.manager.batchDraw();
        };

        cTouchPoint.on('dragmove', function () {
            redraw();
        });

        cTouchPoint.on('dragend', function () {

            var position = circle.getAbsolutePosition();
            var activity = me.manager.getIntersectingActivity(position);

            if (activity != undefined) {
                me.middle = undefined;
                me.Sync();

                if (isend) {
                    me.to.UnregisterTransition(me);
                    me.setTo(activity);
                    me.to.RegisterTransition(me);

                } else {
                    me.from.UnregisterTransition(me);
                    me.setFrom(activity);
                    me.from.RegisterTransition(me);
                }

                me.graph.StoreGraphData();
            }

            me.Draw();
            me.manager.batchDraw();
        });

        return cTouchPoint;

    };

    this._moveActivePoint = function (x, y) {
        this.activePoint.setPosition({ x: x, y: y });
    };

    this._createActivePoint = function (x, y, cTransition) {

        var me = this;

        var draggable = !me.graph.Settings.disableobjectmovements;

        var cActivePoint = new Konva.Group({
            x: x,
            y: y,
            draggable: draggable
        });

        var imageName1 = 'transaction-blak.png';
        var imageName2 = 'transaction-blak.png';
        var textvalue = '';

        var triggertype = this.item.Trigger.Type.toLowerCase();

        if (triggertype === 'auto') {
            textvalue += 'A';//'⚡ 🚀 🚦';
            imageName2 = 'transaction-next.png';
        }
        else if (triggertype === 'command') {
            textvalue += 'C'; //👨🏻‍💼
            imageName2 = 'transaction-command.png';
        }
        else if (triggertype === 'timer' || triggertype === 'timerexpired') {
            textvalue += 'T';//'⏱ ⚡🚩 🚧';
            imageName2 = 'transition-always.png'; 
        }



        

        switch (textvalue) {
            case 'A':
                imageName1 = 'transition-auto.png';
                break;
            case 'C':
                imageName1 = 'transition-command.png';
                break;
            case 'T':
                imageName1 = 'transition-timer.png';
                break;
        }

        
        

        var conditiontype = this.item.Conditions[0].Type.toLowerCase();
        if (conditiontype === 'always') {
            
            textvalue += 'A';
        }
        else if (conditiontype === 'action') {
            imageName2 = 'transition-always.png';
            textvalue += 'C';
        }
        else if (conditiontype === 'otherwise') {
            imageName2 = 'transition-otherwise.png';
            textvalue += 'O';
        }
        

        ////if (textvalue == "AA") { textvalue = ""; }
        ////if (textvalue == "CA") { textvalue = ""; }
        ////if (textvalue == "TA") { textvalue = "     if"; }

        ////if (textvalue == "AC") { textvalue = "     if"; }
        ////if (textvalue == "CC") { textvalue = "     if"; }
        ////if (textvalue == "TC") { textvalue = "     if"; }

        ////if (textvalue == "AO") { textvalue = "     else"; }
        ////if (textvalue == "CO") { textvalue = "     else"; }
        ////if (textvalue == "TO") { textvalue = "     else"; }


        //if (me.item.Trigger != undefined && me.item.Trigger.Command != undefined && me.item.Trigger.Type === 'Command') {
        //    if (Array.isArray(me.item.Restrictions) && me.item.Restrictions.length > 0) {
        //        me.item.Restrictions.forEach(function (item) {
        //            if (item.Actor != undefined) {
        //                var str = item.Actor.Name;
        //                if (item.Type == "Restrict") {
        //                    textvalue += '🔒';
        //                }
        //                else {
        //                    textvalue += '🔓';
        //                }
        //                textvalue = textvalue.trim();
        //            }
        //        });
        //    }
        //}

        //if (textvalue == "AAR") { textvalue = "  ⚡⛔"; }
        //if (textvalue == "ACR") { textvalue = "⚡if⛔"; }
        //if (textvalue == "AOR") { textvalue = "⚡else⛔"; }
        //if (textvalue == "CAR") { textvalue = "  👨🏻‍💼⛔"; }
        //if (textvalue == "CCR") { textvalue = "👨🏻‍💼if⛔"; }
        //if (textvalue == "COR") { textvalue = "👨🏻‍💼else⛔"; }
        //if (textvalue == "TAR") { textvalue = "  ⏱⛔"; }
        //if (textvalue == "TCR") { textvalue = "⏱if⛔"; }
        //if (textvalue == "TOR") { textvalue = "⏱else⛔"; }


        var circle = new Konva.Rect({
            x: -32,//textvalue.length == 5 ? - 32 : -26,
            y: -15,
            width: 75,
            height: 37,
            fill: me.GetColor(),
            cornerRadius: 15
        });


        cActivePoint.add(circle);

        var image = new Konva.Image({
            x: imageName2 == 'transaction-blak.png' ? -12 : -23,
            y: -12,
            image: WorkflowDesignerCommon.loadImage(this.graph.Settings.imagefolder + imageName1),
            width: 32,
            height: 32
        });
        cActivePoint.add(image);

        var image2 = new Konva.Image({
            x: 3,
            y: -15,
            image: WorkflowDesignerCommon.loadImage(this.graph.Settings.imagefolder + imageName2),
            width: 32,
            height: 32
        });
        cActivePoint.add(image2);
        

        //var text = new Konva.Text({
        //    x: textvalue.length == 5 ? -32 : -26,
        //    y: -7,
        //    text: textvalue,
        //    fontSize: 20,
        //    fontFamily: 'Arial',
        //    fill: '#FFFFFF',
        //    fontStyle: 'bold'
        //});
        //cActivePoint.add(text);

        cActivePoint.transition = cTransition;

        var redraw = function (d, r) {
            var point = me.graph.CorrectPossition(cActivePoint.getAbsolutePosition(), me.manager.Layer);
            me.middle = point;

            me.DrawTransition();

            if (!me.graph.Settings.readonly) {
                me.DrawTouchPoints();
            }

            if (d) {
                me.DrawActivePoint();
                me.Sync();
            }

            me.manager.batchDraw();
        };

        var onclick = function (e) {
            if (me.graph.Settings.disableobjectmovements)
                return;

            var tmpSelect = me.selected;

            if (!e.evt.ctrlKey)
                me.graph.DeselectAll();

            if (tmpSelect)
                me.Deselect();
            else
                me.Select();

            if (me.activePoint.ToolTip != undefined) {
                me.activePoint.ToolTip.hide();
            }

            me.graph.onSelectionChanged();
            me.manager.batchDraw();
        };

        cActivePoint.on('click', onclick);
        cActivePoint.on('touchend', onclick);

        cActivePoint.on('dblclick', function () {
            me.graph.DeselectAll();
            me.Select();
            me.manager.batchDraw();
            if (me.graph.Settings.notshowwindows)
                return;
            me.ShowProperties();
        });

        cActivePoint.on('dragstart', function () {
            if (me.graph.Settings.disableobjectmovements)
                return;

            if (me.activePoint.ToolTip != undefined) {
                me.activePoint.ToolTip.hide();
            }
        });

        cActivePoint.on('dragmove', function () {
            if (me.graph.Settings.disableobjectmovements)
                return;
            redraw(false);
        });

        cActivePoint.on('dragend', function () {
            if (me.graph.Settings.disableobjectmovements)
                return;
            redraw(true);
        });

        if (me.graph.getParam("exinfo") == true) {
            me.createExInfo(cActivePoint);
        }
        else {
            var tooltiptext = this.item.Trigger.Type;
            if (me.item.Trigger != undefined && me.item.Trigger.Command != undefined && me.item.Trigger.Type === 'Command')
                tooltiptext = 'Comando: ' + me.item.Trigger.Command.Name;

            if (me.item.Trigger != undefined && me.item.Trigger.Timer != undefined && me.item.Trigger.Type === 'Timer')
                tooltiptext += ' ' + me.item.Trigger.Timer.Name;

            tooltiptext += '\r\n' + this.item.Conditions[0].Type;
            if (me.item.Conditions[0] != undefined && me.item.Conditions[0].Type === 'Action') {
                tooltiptext = 'Condición: ' + me.item.Conditions[0].Action.ActionName;
            }

            WorkflowDesignerTooltip(me.manager.APLayer, cActivePoint, tooltiptext, 17);
        }
        return cActivePoint;
    };

    this.createExInfo = function (cActivePoint) {
        var tooltiptext = "";

        if (me.item.Trigger != undefined && me.item.Trigger.Command != undefined && me.item.Trigger.Type === 'Command') {
            if (Array.isArray(me.item.Restrictions) && me.item.Restrictions.length > 0) {
                me.item.Restrictions.forEach(function (item) {
                    if (item.Actor != undefined) {
                        var str = item.Actor.Name;
                        if (item.Type == "Restrict") {
                            str = "(" + str + ")";
                        }

                        if (tooltiptext.length > 0)
                            tooltiptext += ", ";
                        tooltiptext += str;
                    }
                });
            }

            if (tooltiptext.length > 0)
                tooltiptext += " -> ";

            tooltiptext += me.item.Trigger.Command.Name;
        }

        if (me.item.Trigger != undefined && me.item.Trigger.Timer != undefined && me.item.Trigger.Type === 'Timer') {
            tooltiptext += ' ' + me.item.Trigger.Timer.Name;

            var value = me.item.Trigger.Timer.Value;
            if (value != undefined && value != "")
                tooltiptext += ' ' + value;
        }

        if (tooltiptext.length > 0) {
            var textctrl = new Konva.Text({
                x: 0,
                y: -30,
                text: tooltiptext,
                fontFamily: 'Arial',
                fontSize: 12,
                fill: '#4A4A4A',
                fontStyle: 'bold'
            });

            textctrl.setX(- Number(textctrl.getWidth() / 2));
            cActivePoint.add(textctrl);
        }

        var tooltiptext2 = "";
        if (Array.isArray(me.item.Conditions) && me.item.Conditions.length > 0) {
            me.item.Conditions.forEach(function (item) {
                if (item.Action != undefined) {
                    var str = item.Action.ActionName;
                    if (item.ConditionInversion == true) {
                        str = "(" + str + ")";
                    }

                    if (tooltiptext2.length > 0)
                        tooltiptext2 += ", ";
                    tooltiptext2 += str;
                }
            });
        }

        if (tooltiptext2.length > 0) {
            var textctrl = new Konva.Text({
                x: 0,
                y: 25,
                text: tooltiptext2,
                fontFamily: 'Arial',
                fontSize: 12,
                fill: '#4A4A4A',
                fontStyle: 'bold'
            });

            textctrl.setX(- Number(textctrl.getWidth() / 2));
            cActivePoint.add(textctrl);
        }
    };

    this._getLineLength = function (x1, y1, x2, y2) {
        return Math.sqrt(Math.pow((x2 - x1), 2) + Math.pow((y2 - y1), 2));
    };

    this._getBendingKoeff = function (x1, y1, x2, y2, xap, yap) {

        var a = y1 - y2;
        var b = x2 - x1;
        var c = (x1 * y2 - x2 * y1);
        if (b <= 0) {
            a = -a;
            b = -b;
            c = -c;
        }

        var yapn = -(c + a * xap) / b;
        var sign = yapn < yap ? -1 : 1;

        var len1 = this._getLineLength(x1, y1, x2, y2);
        var xc = (x1 + x2) / 2;
        var yc = (y1 + y2) / 2;
        var len2 = this._getLineLength(xc, yc, xap, yap);
        var bending = len2 / len1 * sign;

        if (b == 0)
            bending = -bending;

        return bending;
    };

    this.getIntersectingRect = function (rect) {
        var point = this.activePoint.getAbsolutePosition();
        if (point.x >= rect.xl && point.x < rect.xr && point.y >= rect.yl && point.y < rect.yr)
            return true;
        return false;
    };

    this.ShowProperties = function () {

        var labels = WorkflowDesignerConstants.TransitionFormLabel;

        var params = {
            type: 'form',
            title: labels.Title,
            data: this.item,
            readonly: this.graph.Settings.readonly,
            elements: [
                {
                    type: "group", elements: [
                        { name: labels.Name, field: "Name", type: "input", width: "100%" },
                        //{ name: labels.Classifier, field: "Classifier", type: "select", width: "100%", datasource: ['NotSpecified', 'AntWay', 'Direct', 'Reverse'] }
                        { name: labels.Classifier, field: "Classifier", type: "select", width: "100%", datasource: ['NotSpecified', 'TimeInterval'] }
                     ]
                },
                {
                    type: "group", elements: [
                        { name: labels.From, field: "From.Name", type: "select", displayfield: 'Name', datasource: me.graph.data.Activities, width: "100%" },
                        { name: labels.To, field: "To.Name", type: "select", displayfield: 'Name', datasource: me.graph.data.Activities, width: "100%" }]
                },
                {
                    field: "Trigger", code: 'trigger', type: "form", datadefault: { Type: 'Command' }, elements: [
                        {
                            type: "group", elements: [
                                { name: labels.Trigger, code: 'triggertype', field: "Type", type: "select", datasource: ['Auto', 'Command', 'Timer', 'TimerExpired'] },
                                { name: labels.TriggerCommand, code: 'triggercommand', field: "Command.Name", type: "select", displayfield: 'Name', datasource: me.graph.data.Commands },
                                { name: labels.TriggerTimer, code: 'triggertimer', field: "Timer.Name", type: "select", displayfield: 'Name', datasource: me.graph.data.Timers }
                            ]
                        }
                    ]
                }, {
                    name: labels.Restrictions, field: "Restrictions", code: 'restrictions', type: "table", datadefault: { Type: 'Allow' }, elements: [
                        { name: labels.RestrictionsType, code: 'resttype', field: "Type", type: "select", datasource: ['Allow', 'Restrict'] },
                        { name: labels.RestrictionsActor, code: 'restactor', field: "Actor.Name", type: "select", displayfield: 'Name', datasource: me.graph.data.Actors }
                    ]
                },
                {
                    type: "group", elements: [
                        { name: labels.AllowConcatenationType, field: "AllowConcatenationType", type: "select", datasource: ['And', 'Or'] },
                        { name: labels.RestrictConcatenationType, field: "RestrictConcatenationType", type: "select", datasource: ['And', 'Or'] }]
                },
                {
                    name: labels.Condition, field: "Conditions", code: 'condition', type: "table", datadefault: { Type: 'Always', ResultOnPreExecution: 'Null' }, elements: [
                        { name: labels.ConditionType, code: 'conditiontype', field: "Type", type: "select", datasource: ['Always', 'Action', 'Otherwise'] },
                        { name: labels.ConditionAction, code: 'conditionaction', field: "Action.ActionName", type: "select", datasource: me.graph.getConditionNames() },
                        {
                            name: labels.ConditionActionParameter,
                            code: 'conditionactionparameter',
                            field: "Action.ActionParameter",
                            type: "json",
                            openautocompleteonclick: true,
                            datasource: function (request, response) {
                                var tr = $(this.element[0]).closest("tr");
                                var conditionName = tr.find("[name=conditionaction]")[0].value;
                                response(me.graph.getAutoCompleteSuggestions("conditionparameter", conditionName, request.term));
                            }
                        },
                        { name: labels.ConditionInversion, code: 'conditioninversion', field: "ConditionInversion", type: "checkbox" },
                        { name: labels.ResultOnPreExecution, code: 'conditionresult', field: "ResultOnPreExecution", type: "select", datasource: ['True', 'False'] }
                    ],
                    onrowadded: function (row) {
                        var conditiontype = row.find('[name=conditiontype]');
                        var conditionactionrow = row.find('[name=conditionaction]');//.parent().parent();
                        var conditionresultrow = row.find('[name=conditionresult]');//.parent().parent();
                        var conditionactionparameterrow = row.find('[name=conditionactionparameter]').parent().parent(); //combined json control
                        var conditioninversionrow = row.find('[name=conditioninversion]').parent();//.parent();
                        var checkConditionType = function () {
                            var type = conditiontype[0].value;
                            if (type == 'Action') {
                                conditionactionrow.show();
                                conditionresultrow.show();
                                conditionactionparameterrow.show();
                                conditioninversionrow.show();

                            }
                            else {
                                conditionactionrow.hide();
                                conditionresultrow.hide();
                                conditionactionparameterrow.hide();
                                conditioninversionrow.hide();
                            }
                        }
                        conditiontype.on('change', checkConditionType);
                        checkConditionType();
                    }

                },
                {
                    type: "group", elements: [
                        { name: labels.ConditionsConcatenationType, field: "ConditionsConcatenationType", type: "select", datasource: ['And', 'Or'] }
                    ]
                },
                {
                    type: "group", elements: [
                        { name: labels.IsFork, field: "IsFork", code: 'isfork', type: "checkbox" },
                        { name: labels.MergeViaSetState, field: "MergeViaSetState", code: 'mergeviasetstate', type: "checkbox" },
                        { name: labels.DisableParentStateControl, field: "DisableParentStateControl", code: 'disableparentstatecontrol', type: "checkbox" }
                    ]
                }
            ],

            renderFinalFunc: function (control, f) {
                var restrictions = control.find('[name=restrictions]').parent();
                var triggertype = control.find('[name=triggertype]');
                var isfork = control.find('[name=isfork]');
                var triggercommandrow = control.find('[name=triggercommand]');
                var triggertimerrow = control.find('[name=triggertimer]');
                var concatenationtr = control.find('[name=AllowConcatenationType]').parent().parent();
                var conditionsconcatenationtr = control.find('[name=ConditionsConcatenationType]').parent().parent();
                var mergeviasetstaterow = control.find('[name=mergeviasetstate]').parent().parent();
                var disableparentstatecontrolrow = control.find('[name=disableparentstatecontrol]').parent().parent();

                var checkTriggerType = function () {

                    var getLabel = function (control) {
                        return control.prev();
                    };

                    var type = triggertype[0].value;
                    if (type == 'Command') {
                        triggercommandrow.show();
                        getLabel(triggercommandrow).show();

                        triggertimerrow.hide();
                        getLabel(triggertimerrow).hide();

                        restrictions.show();
                    }
                    else if (type == 'Timer') {
                        triggercommandrow.hide();
                        getLabel(triggercommandrow).hide();

                        triggertimerrow.show();
                        getLabel(triggertimerrow).show();

                        restrictions.hide();
                        concatenationtr.hide();
                    }
                    else {
                        triggercommandrow.hide();
                        getLabel(triggercommandrow).hide();

                        triggertimerrow.hide();
                        getLabel(triggertimerrow).hide();

                        restrictions.hide();
                        concatenationtr.hide();
                    }

                    WorkflowDesignerCommon.modal(f.window, "refresh");
                }
                triggertype.on('change', checkTriggerType);
                checkTriggerType();

                var checkIsFork = function () {
                    var isFork = isfork[0].checked;
                    if (isFork) {
                        mergeviasetstaterow.show();
                        disableparentstatecontrolrow.show();
                    } else {
                        mergeviasetstaterow.hide();
                        disableparentstatecontrolrow.hide();
                    }
                }
                isfork.on('change', checkIsFork);
                checkIsFork();

                var allowConcatenationType = concatenationtr.find('[name=AllowConcatenationType]')[0].value.toLowerCase();
                var restrictConcatenationType = concatenationtr.find('[name=RestrictConcatenationType]')[0].value.toLowerCase();
                var conditionConcatenationType = conditionsconcatenationtr.find('[name=ConditionsConcatenationType]')[0].value.toLowerCase();

                if (allowConcatenationType === 'and' && restrictConcatenationType === 'and') {
                    concatenationtr.hide();
                }

                var restrictionstd = control.find('[name=restrictions]').parent();
                var restrictionsbtn = $('<a class="btnConcatParameters"></a>');
                restrictionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.ShowConcatParameters;
                restrictionsbtn.on('click', function () {
                    if (concatenationtr.is(':visible')) {
                        concatenationtr.hide();
                        restrictionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.ShowConcatParameters;
                    } else {
                        concatenationtr.show();
                        restrictionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.HideConcatParameters;
                    }
                    WorkflowDesignerCommon.modal(f.window, "refresh");
                });
                restrictionstd.append('&nbsp;');
                restrictionstd.append(restrictionsbtn);

                if (conditionConcatenationType === 'and')
                    conditionsconcatenationtr.hide();

                var conditionstd = control.find('[name=condition]').parent();
                var conditionsbtn = $('<a class="btnConcatParameters"></a>');
                conditionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.ShowConcatParameters;
                conditionsbtn.on('click', function () {
                    if (conditionsconcatenationtr.is(':visible')) {
                        conditionsconcatenationtr.hide();
                        conditionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.ShowConcatParameters;
                    } else {
                        conditionsconcatenationtr.show();
                        conditionsbtn[0].innerText = WorkflowDesignerConstants.TransitionFormLabel.HideConcatParameters;
                    }
                    WorkflowDesignerCommon.modal(f.window, "refresh");
                });
                conditionstd.append('&nbsp;');
                conditionstd.append(conditionsbtn);
                WorkflowDesignerCommon.modal(f.window, "refresh");
            },
            graph: me.graph
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;

            isValid &= formControl.CheckRequired([data], ['Name'], WorkflowDesignerConstants.FieldIsRequired);
            isValid &= formControl.CheckRequired([data], ['Classifier'], WorkflowDesignerConstants.FieldIsRequired);
            var reqfields = ['Type'];
            if (data.Trigger.Type == 'Command') {
                reqfields.push('Command.Name');
            }
            else if (data.Trigger.Type == 'Timer') {
                reqfields.push('Timer.Name');
            }

            isValid &= formControl.CheckRequired([data.Trigger], reqfields, WorkflowDesignerConstants.FieldIsRequired);

            data.Conditions.forEach(function (c) {
                reqfields = ['Type'];
                if (c.Type == 'Action') {
                    reqfields.push('Action.ActionName');
                }

                isValid &= formControl.CheckRequired([c], reqfields, WorkflowDesignerConstants.FieldIsRequired);

                if (c.Type == 'Always' && data.Conditions.length > 1) {
                    isValid = false;
                    formControl.ControlAddError(c['control_Type'], WorkflowDesignerConstants.AlwaysConditionShouldBeSingle);
                }
                else if (c.Type == 'Otherwise' && data.Conditions.length > 1) {
                    isValid = false;
                    formControl.ControlAddError(c['control_Type'], WorkflowDesignerConstants.OtherwiseConditionShouldBeSingle);
                }
            });
            var c = data.control_Conditions.parent().children('label');//$(data.control_Conditions.parent().parent().children()[0]).children('label');

            if (!data.Conditions.length > 0) {
                c.attr('title', WorkflowDesignerConstants.TransitionFormLabel.ConditionsListShouldNotBeEmpty);
                c.css("cssText", "color: red !important;");
                isValid = false;
            } else {
                c.attr('title', undefined);
                c.css('color', '');
            }

            me.graph.data.Transitions.forEach(function (a) {
                if (a != me.item && a.Name == data.Name) {
                    isValid = false;
                    formControl.ControlAddError(data.control_Name, WorkflowDesignerConstants.FieldMustBeUnique);
                }
            });

            if (!formControl.CheckRequired(data.Restrictions, ['Type', 'Actor.Name'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }

            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);
                me.item.Name = data.Name;
                me.item.From = { Name: data.From.Name };
                me.item.To = { Name: data.To.Name };
                me.item.Classifier = data.Classifier;
                me.item.Restrictions = data.Restrictions;
                me.item.Trigger = data.Trigger;
                me.item.Conditions = data.Conditions;
                me.item.IsFork = data.IsFork;
                me.item.MergeViaSetState = data.MergeViaSetState;
                me.item.DisableParentStateControl = data.DisableParentStateControl;
                me.item.ConditionsConcatenationType = data.ConditionsConcatenationType;
                me.item.AllowConcatenationType = data.AllowConcatenationType;
                me.item.RestrictConcatenationType = data.RestrictConcatenationType;
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
        me.item.DesignerSettings.Bending = me.bending;
        if (me.middle != undefined) {
            me.item.DesignerSettings.X = me.middle.x;
            me.item.DesignerSettings.Y = me.middle.y;
        }
    };

    this.destroy = function () {
        this.control.destroy();
        this.activePoint.destroy();
        this.touchpoints.forEach(function (tp) { tp.destroy() });

        if (this.bar != undefined)
            this.bar.destroy();
    };
}

function WorkflowDesignerTransitionManagerTempControl(parameters) {
    this.x = parameters.x;
    this.y = parameters.y;
    this.manager = parameters.manager;
    this.control = undefined;

    this.Draw = function (xe, ye) {
        this.control = new Konva.Group({
            x: 0,
            y: 0,
            rotation: 0
        });
        this.line = new Konva.Line({
            points: [this.x, this.y, xe, ye],
            stroke: '#FFCC99',
            strrokeWidth: 1
        });

        var angle = Math.atan2(ye - this.y, xe - this.x);
        this.arrow = WorkflowDesignerCommon.createArrowByAngle(xe, ye, angle, 20, '#FFCC99');

        this.control.add(this.line);
        this.control.add(this.arrow);

        this.manager.Layer.add(this.control);
    };

    this.Redraw = function (p) {
        this.line.setPoints([this.x, this.y, p.x, p.y]);
        var angle = Math.atan2(p.y - this.y, p.x - this.x);
        WorkflowDesignerCommon.updateArrowByAngle(this.arrow, p.x, p.y, angle, 20, '#FFCC99');
    };

    this.Delete = function () {
        this.control.destroy();
    };
}