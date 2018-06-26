function WorkflowDesignerForm(parameters) {
    this.type = 'WorkflowDesignerForm';

    this.parameters = parameters;
    this.id = WorkflowDesignerCommon.createUUID();

    this.isReadOnly = function () {
        return this.parameters.readonly;
    }
    
    this.showModal = function (saveFunc, allowMultiple) {
        var me = this;
        me.window = $('<div class="ui modal WorkflowDesignerDialog"></div>');

        if (WorkflowDesignerConstants.FormMaxHeight != undefined && WorkflowDesignerConstants.FormMaxHeight != "")
            me.window.css('max-height', WorkflowDesignerConstants.FormMaxHeight);
        if (this.parameters.width != undefined && this.parameters.width != "")
            me.window.width(this.parameters.width);

        me.window.id = this.id;
        var controls = undefined;
        if (this.parameters.type === 'table') {
            controls = this.generateTable(this.parameters);
        }
        else if (this.parameters.type === 'form') {
            controls = this.generateForm(this.parameters);
        }

        if (controls == undefined)
            controls = new Array();

        if (me.parameters.top != undefined) {
            controls.unshift((me.parameters.top));
        }

        if (me.parameters.bottom != undefined) {
            controls.push((me.parameters.bottom));
        }

        if (this.parameters.renderFinalFunc != undefined) {
            this.parameters.renderFinalFunc(controls, me);
        }

        var originalData = me.getEditData(me.parameters);

        me.window.append($('<div class="header">' + this.parameters.title + '</div>'));
        me.window.append($('<div class="content scrolling"></div>').append(controls));

        var actions = $('<div class="actions"></div>');
        if (me.isReadOnly()) {
            actions.append('<div class="ui secondary  cancel button">' + WorkflowDesignerConstants.ButtonTextClose + '</div>');
        }
        else {
            actions.append('<div class="ui primary ok button">' + WorkflowDesignerConstants.ButtonTextSave + '</div>');
            actions.append('<div class="ui secondary cancel button">' + WorkflowDesignerConstants.ButtonTextCancel + '</div>');
        }
        me.window.append(actions);

        var onApprove = function () {
            me.ClearError();
            if (saveFunc != undefined && !me.isReadOnly()) {
                var data = me.getEditData(me.parameters);
                if (saveFunc(data, me.parameters)) {
                    me.allowDestroy = true;
                    return true;
                }
                return false;
            }
        };

        var onDeny = function () {
            if (!me.isReadOnly()) {
                var data = me.getEditData(me.parameters);
                if (JSON.stringify(originalData) === JSON.stringify(data)) {
                    me.allowDestroy = true;
                    return true;
                }

                me.ConfirmDialog(WorkflowDesignerConstants.CloseWithoutSaving,
                    WorkflowDesignerConstants.ButtonTextYes,
                    function () {
                        me.allowDestroy = true;
                        WorkflowDesignerCommon.modal(me.window, "hide");
                    },
                    WorkflowDesignerConstants.ButtonTextNo, function () {
                        return true;
                    });
                return false;
            }
        };
        WorkflowDesignerCommon.modal(me.window, { 
            closable: false, 
            onApprove: onApprove, 
            onDeny: onDeny, 
            allowMultiple: allowMultiple,
            onHidden: function(){
                if(me.allowDestroy == true)
                    me.destroy();
            },
            dimmerSettings: {
                opacity: 0.2,
                duration: {
                    show : 0,
                    hide : 0
                  }
            },
            transition: 'fade'
        });
        WorkflowDesignerCommon.modal(me.window, "show");
        WorkflowDesignerCommon.modal(me.window, "refresh");
    };

    this.destroy = function(){
        $('.WorkflowDesignerDialogautoComplete').remove();
        this.window.remove();
    };
    this.ConfirmDialog = function (message, button1Text, button1Click, button2Text, button2Click) {
        var w = $('<div class="ui mini modal WorkflowDesignerConfirmDialog"></div>');
        w.append($('<div class="header">' + WorkflowDesignerConstants.DialogConfirmText + '</div>'));
        w.append($('<div class="content scrolling"><p>' + message + '</p></div>'));

        var actions = $('<div class="actions"></div>')
            .append('<div class="ui primary ok button">' + button1Text + '</div>')
            .append('<div class="ui secondary  cancel button">' + button2Text + '</div>');

        w.append(actions);
        WorkflowDesignerCommon.modal(w,{
            onApprove: function () {
                button1Click();
            },
            onDeny: function () {
                button2Click();
            },
            allowMultiple: true,
            dimmerSettings: {
                opacity: 0.2,
                duration: {
                    show : 0,
                    hide : 0
                  }
            },
            transition: 'fade'
        });
        WorkflowDesignerCommon.modal(w, "show");
    };

    this.InfoDialog = function (header, message, css) {
        var divClass = "ui modal WorkflowDesignerConfirmDialog";
        if(css != undefined)
        divClass += " " + css;

        var w = $('<div class="'+ divClass +'"></div>');
        w.append($('<div class="header">' + header + '</div>'));
        w.append($('<div class="content scrolling"><p>' + message + '</p></div>'));

        var actions = $('<div class="actions"></div>')
            .append('<div class="ui primary ok button">' + WorkflowDesignerConstants.EditCodeLabel.OK + '</div>');

        w.append(actions);
        WorkflowDesignerCommon.modal(w, {
            allowMultiple: true,
            dimmerSettings: {
                opacity: 0.2,
                duration: {
                    show : 0,
                    hide : 0
                  }
            },
            transition: 'fade'
        });
        WorkflowDesignerCommon.modal(w, "show");
    };

    this.getEditData = function (p) {
        var me = this;
        var data;
        if (p.type == 'form' || p.type == 'group') {
            data = {};
            p.elements.forEach(function (element) {

                if(element.field != undefined)
                    data['control_' + element.field] = element.control;

                if (element.type == 'table' || element.type == 'form') {
                    data[element.field] = me.getEditData(element);
                }
                else  if (element.type == 'group') {
                    me.objAssign(data, me.getEditData(element));
                }
                else {
                    me.SetValueByPropertyName(data, element.field,
                        me.getEasyControlValue(element));
                }
            });
        }
        else if (p.type == 'table') {
            if(data == undefined)
                data = [];
            var table = p.control;
            p.elements.forEach(function (element) {
                var code = me.getElementCode(element);
                var filter = '[name=' + code + ']';
                var elementControls = table.find(filter);
                if (elementControls != undefined) {
                    for (var i = 0; i < elementControls.length; i++) {
                        if (data[i] == undefined)
                            data[i] = {};
                        data[i]['control_' + element.field] = elementControls[i];

                        if (element.type == 'table' || element.type == 'form') {
                            data[i][element.field] = me.getEditData({ type: element.type, control: $(elementControls[i]), elements: element.elements });
                        }
                        else {
                            me.SetValueByPropertyName(data[i], element.field,
                                me.getEasyControlValue({ type: element.type, control: elementControls[i] }));
                        }
                    }
                }
            });

            if (p.keyproperty) {
                var trArray = table.children('tbody').children('tr');

                for (var i = 0; i < trArray.length; i++) {
                    if (data[i] == undefined)
                        data[i] = {};

                    data[i].keyproperty = $(trArray[i]).attr('keyproperty');
                }
            }
        }

        return data;
    }

    this.generateForm = function (p, prefix) {
        var me = this;

        var parent = p.type == 'group' ?  $('<div class="fields">') : $('<div class="ui form">');
        
        parent.attr('name', me.getElementCode(p));
        var res = new Array();
        p.elements.forEach(function (element) {
            var row = $('<div class="field">');

            if(element.width != undefined){
                row.width(element.width);
            }

            if (prefix == undefined) prefix = '';
            var elementPrefix = prefix + '_' + element.field;

            var label = undefined;
            if(element.name != undefined){
                label = $('<label></label>');
                label[0].innerHTML = element.name;
                row.append(label);
            }

            if (element.type == 'table') {

                if (element.fieldFunc) {
                    element.data = element.fieldFunc(p.data);
                } else {
                    element.data = p.data[element.field];
                }

                var table = me.generateTable(element, elementPrefix);
                row.append(table);
            }
            else if (element.type == 'form') {

                if (element.fieldFunc) {
                    element.data = element.fieldFunc(p.data);
                } else {
                    element.data = p.data[element.field];
                }

                var table = me.generateForm(element, elementPrefix);
                row.append(table);
            }
            else if (element.type == 'group') {

                if (element.fieldFunc) {
                    element.data = element.fieldFunc(p.data);
                } else {
                    element.data = p.data;
                }

                var table = me.generateForm(element, elementPrefix);
                row.append(table);
            }
            else {
                var control = me.generateEasyControls(element,
                    me.GetValueByPropertyName(p.data, element.field),
                    elementPrefix);
                if(label != undefined)
                    label[0].for = control[0].id;
                element.control = control[0];
                row.append(control[0]);
            }

            res.push(row);
        });
        p.control = parent;
        parent.append(res);
        return parent;
    };
    this.generateTable = function (p, prefix) {
        var me = this;

        var table = $('<table class="ui form WorkflowDesignerTable"></table>');
        table.attr('name', me.getElementCode(p));

        var tabvarhead = $('<thead></thead>');
        var headtr = $('<tr></tr>');

        p.elements.forEach(function (c) {
            var headth = $('<th></th>');
            headth[0].innerHTML = c.name;
            if (c.width != undefined)
                headth[0].width = c.width;
            headtr.append(headth);
        });

        if (!this.isReadOnly()) {
            headtr.append('<th></th>');
        }

        tabvarhead.append(headtr);
        table.append(tabvarhead);

        var addrow = function (item) {

            if (p.beforerowadded != undefined)
                p.beforerowadded(item, me);

            var row = $('<tr></tr>');
            if (p.keyproperty)
                row.attr('keyproperty', item[p.keyproperty]);

            if (prefix == undefined) prefix = '';
            var prefixElement = prefix + WorkflowDesignerCommon.createUUID();

            p.elements.forEach(function (e) {
                if (e.type == 'table') {
                    if (e.fieldFunc) {
                        e.data = e.fieldFunc(item);
                    } else {
                        e.data = item[e.field];
                    }

                    var table = me.generateTable(e, prefixElement);
                    row.append($('<td></td>').append(table));
                }
                else {
                    var control = me.generateEasyControls(e, me.GetValueByPropertyName(item, e.field), prefixElement, item);
                    row.append($('<td></td>').append(control));
                }
            });

            if (!me.isReadOnly()) {
                var deletebutton = $('<a class="btnDelete"></a>');
                deletebutton[0].innerHTML = WorkflowDesignerConstants.ButtonTextDelete;
                deletebutton[0].href = "#"
                deletebutton.on('click', function () {
                    if (p.onrowdelete != undefined){
                        if(p.onrowdelete(row, me) == false){
                            return false;
                        }
                    }

                    row.remove();
                    WorkflowDesignerCommon.modal(me.window, "refresh");
                    return false;
                });
                row.append($('<td></td>').append(deletebutton));
            }

            table.append(row);

            if (p.onrowadded != undefined)
                p.onrowadded(row, me);

        };

        if (p.data != undefined) {
            p.data.forEach(function (item) {
                addrow(item);
            });
        }

        p.control = table;

        var res = new Array();
        res.push(table);
        if (!this.isReadOnly()) {
            var createbutton = $('<a class="btnAdd"></a>');
            createbutton[0].innerHTML = WorkflowDesignerConstants.ButtonTextCreate;
            createbutton[0].href = "#";
            createbutton.on('click', function () {
                var defaultdata = {};
                if (p.datadefault)
                    defaultdata = p.datadefault;
                addrow(defaultdata);
                WorkflowDesignerCommon.modal(me.window, "refresh");
                return false;
            });
            res.push(createbutton);
        }
        return res;
    };

    this.generateEasyControls = function (p, value, prefix, item) {
        var me = this;
        if (p.type === 'input') {
            var control = $('<input type="text"></input>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);

            if (value != undefined)
                control[0].value = value;

            if (me.isReadOnly())
                control.attr('readonly', true);

            this.addAutoComplete(p, control);

            return control;
        } else if (p.type === 'checkbox') {
            var control = $('<input type="checkbox"></input>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].checked = value;
            control[0].name = me.getElementCode(p);

            if (me.isReadOnly())
                control.attr('disabled', "disabled");

            control = $('<div class="ui checkbox"></div>').append(control).append('<label></label>');
            return control;
        } else if (p.type == 'select') {
            var control = $('<select class="ui selection dropdown"></select>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);
            control.append($('<option></option>'));
            if (p.datasource != undefined) {
                p.datasource.forEach(function (item) {
                    var option = $('<option></option>');
                    if (p.displayfield == undefined) {
                        option[0].value = item;
                        option[0].innerHTML = item;
                    } else {
                        option[0].value = item[p.displayfield];
                        option[0].innerHTML = item[p.displayfield];
                    }

                    if (option[0].value == value) {
                        option[0].selected = "selected";
                    }

                    if (me.isReadOnly())
                        control.attr('disabled', "disabled");

                    control.append(option);
                });
            }


            return control;
        } else if (p.type === 'textarea') {
            var control = $('<textarea rows="6" style="width: 100%;"></textarea>');
            control[0].id = me.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);

            if (value != undefined)
                control[0].value = value;

            if (this.isReadOnly())
                control.attr('readonly', true);

            return control;
        } else if (p.type === 'json') {
            return me.generateJSONControl(p,value, prefix, item);
        } else if (p.type === 'code') {
            return me.generateCodeControl(p,value, prefix, item);
        }
    };

    this.generateJSONControl = function(p, value, prefix, item){
        var control = $('<input type="text"></input>');
        control[0].id = this.generateid(p.field, prefix);
        control[0].name = me.getElementCode(p);

        if (value != undefined)
            control[0].value = value;

        if (me.isReadOnly()) {
            control.attr('readonly', true);
        }

        var button = $('<a class="btnCodeActions"></a>');
        button[0].id = control[0].id + '_button';

        var form = $('<div class="ui modal WorkflowDesignerDialogChild">' +
            '<div class="header">' + WorkflowDesignerConstants.EditJSONLabel.Title + '</div>' +
            '<div id="' + control[0].id + '_editor' + '" style="height:' + WorkflowDesignerConstants.EditJSONSettings.CodeHeight + 'px">' +
            control[0].value + '</div>' +
            '</div>');
        form[0].id = control[0].id + '_form';

        var actions = $('<div class="actions"></div>');

        if (!control[0].readOnly) {
            var formatbutton = $('<div class="ui button">' + WorkflowDesignerConstants.EditJSONLabel.Format + '</div>');
            formatbutton.click(function () {
                var editor = ace.edit(control[0].id + '_editor');
                var value = (ace.edit(control[0].id + '_editor')).getValue();
                editor.setValue(me.toPrettyJSON(value));
                editor.clearSelection();
            });
            actions.append(formatbutton);
        }

        if (p.getemptytype != undefined && !control[0].readOnly) {
            var getemptytype = $('<div class="ui button">' + WorkflowDesignerConstants.EditJSONLabel.CreateEmptyType + '</div>');
            getemptytype.click(function () {
                var createparameters = p;
                if (createparameters.getemptytype != undefined) {
                    createparameters.getemptytype(me,
                        control[0],
                        function (response) {
                            if (response != undefined && response !== "") {
                                var editor = ace.edit(control[0].id + '_editor');
                                editor.setValue(me.toPrettyJSON(response));
                                editor.clearSelection();
                            }
                        });

                }
            });
            actions.append(getemptytype);
        }

        if (control[0].readOnly) {
            actions.append('<div class="ui secondary cancel button">' + WorkflowDesignerConstants.ButtonTextClose + '</div>');
        }
        else {
            actions.append('<div class="ui primary ok button">' + WorkflowDesignerConstants.ButtonTextSave + '</div>');
            actions.append('<div class="ui secondary cancel button">' + WorkflowDesignerConstants.ButtonTextCancel + '</div>');
        }


        form.append(actions);

        button.on('click',
            function (event) {
                WorkflowDesignerCommon.modal(me.window, "hide");
                WorkflowDesignerCommon.modal(form, {
                    closable: false,
                    allowMultiple: true,
                    onApprove: function () {
                        var value = (ace.edit(control[0].id + '_editor')).getValue();
                        control[0].value = me.toCompactJSON(value);
                    },
                    onHidden: function(){
                        setTimeout(function(){
                            WorkflowDesignerCommon.modal(me.window, "show");
                        }, 10);
                        
                    },
                    dimmerSettings: {
                        opacity: 0.2,
                        duration: {
                            show : 0,
                            hide : 0
                          }
                    },
                    transition: 'fade'
                });
                WorkflowDesignerCommon.modal(form, "show");
                

                var editor = ace.edit(control[0].id + '_editor');
                if (control[0].readOnly) {
                    editor.setOptions({ readOnly: true });
                } else {
                    editor.setOptions({ readOnly: false });
                }

                editor.getSession().setMode("ace/mode/json");
                editor.setValue(me.toPrettyJSON(control[0].value));
                editor.clearSelection();
            });

        this.addAutoComplete(p, control);

        return $('<div style="width:100%;"></div>')
            .append($('<div style="width:16px; float:right; margin-right:7px;margin-top: 10px;"></div>').append(button))
            .append($('<div style="margin-right:30px"></div>').append(control));
    };

    this.generateCodeControl = function (p, value, prefix, item) {
        if (value == undefined)
            value = '';
       
        var control = $('<button class="ui button basic">' + WorkflowDesignerConstants.EditCodeLabel.EditCodeButton + '</button>');
        control[0].id = this.generateid(p.field, prefix);
        control[0].name = me.getElementCode(p);
        control[0].code = {};
        control[0].code.code = (decodeURIComponent(value));
        var usings = (item.Usings);

        if (usings == undefined) {
            usings = me.parameters.graph.data.AdditionalParams.Usings.join(';') + ';';
        } else {
            usings = (decodeURIComponent(usings));
        }

        control[0].code.usings = usings;

        var readonlyStr = me.isReadOnly() ? ' readonly="true"' : '';

        var form = $('<div class="ui large modal WorkflowDesignerDialogChild">');
        form[0].id = control[0].id + '_form';

        form.append('<div class="header">' + WorkflowDesignerConstants.EditCodeLabel.Title + '</div>');
        
        var content = $('<div class="content scrolling"></div>');
        var showUsings = $('<a class="ui button">' + WorkflowDesignerConstants.EditCodeLabel.ShowUsings + '</a>');
        content.append(showUsings);

        var blockusings = $('<div id="' + control[0].id + '_usings' + '" style="padding-top: 6px;display:none"/>');
        blockusings.append('<textarea style="width:100%;height: 100px; max-width:inherit;" id="' + control[0].id + '_usingsedit"' +readonlyStr + '>asdfasdfasd</textarea>');
        content.append(blockusings);
        
        content.append('<div id="' + control[0].id + '_function_upper' +'" />');
        content.append('<div id="' + control[0].id + '_editor' + '" style="height:' + WorkflowDesignerConstants.EditCodeSettings.CodeHeight + 'px" '+ readonlyStr +'></div>');
        content.append('<div id="' + control[0].id + '_function_lower' +'">}</div>');

        showUsings.on('click', function(event){
            if(blockusings.is(':visible')){
                blockusings.hide();
                showUsings[0].innerText = WorkflowDesignerConstants.EditCodeLabel.ShowUsings;
            }
            else{
                blockusings.show();
                showUsings[0].innerText = WorkflowDesignerConstants.EditCodeLabel.HideUsings;
            }

            WorkflowDesignerCommon.modal(form, "refresh");
        });
        
        form.append(content);
        
        var actions = $('<div class="actions"></div>');
        if (!WorkflowDesignerForm.isjava) {
            var compile = $('<div class="ui button">' + WorkflowDesignerConstants.EditCodeLabel.Compile + '</div>');
            actions.append(compile);

            compile.on('click', function () {
                var items = me.getEditData(me.parameters);
                var item = undefined;
                for (var i = 0; i < items.length; i++) {
                    if (items[i].control_ActionCode.id == control[0].id) {
                        item = items[i];
                        break;
                    }
                }

                if (item == undefined)
                    return;

                item.ActionCode =
                    encodeURIComponent(ace.edit(control[0].id + '_editor').getValue());
                item.Usings =
                    encodeURIComponent($('#' + control[0].id + '_usingsedit')[0].value
                        .replace(/(\r\n|\n|\r)/gm, ""));
                var callbackfn = function (response) {
                    var header = response.Success ? WorkflowDesignerConstants.EditCodeLabel.Success : WorkflowDesignerConstants.EditCodeLabel.Error;
                    var content = response.Success ? WorkflowDesignerConstants.EditCodeLabel.CompileSucceeded : response.Message;
                    me.InfoDialog(header, content, response.Success ? "mini" : undefined);
                    return false;
                }

                me.parameters.graph.designer.compile(item, callbackfn);
            });
        }

    
        if (control[0].readOnly) {
            actions.append('<div class="ui secondary cancel button">' + WorkflowDesignerConstants.ButtonTextClose + '</div>');
        }
        else {
            actions.append('<div class="ui primary ok button">' + WorkflowDesignerConstants.ButtonTextSave + '</div>');
            actions.append('<div class="ui secondary cancel button">' + WorkflowDesignerConstants.ButtonTextCancel + '</div>');
        }

        form.append(actions);

        control.on('click',
            function (event) {
                WorkflowDesignerCommon.modal(form, {
                    closable: false,
                    allowMultiple: true,
                    onApprove: function () {
                        control[0].code = {};
                        control[0].code.code = (ace.edit(control[0].id + '_editor').getValue());
                        control[0].code.usings = ($('#' + control[0].id + '_usingsedit')[0].value
                             .replace(/(\r\n|\n|\r)/gm, ""));
                    },
                    onHidden: function(){
                        setTimeout(function(){
                            WorkflowDesignerCommon.modal(me.window, "show");
                        }, 10);
                        
                    },
                    dimmerSettings: {
                        opacity: 0.2,
                        duration: {
                            show : 0,
                            hide : 0
                          }
                    },
                    transition: 'fade'
                });

                var editor = ace.edit(control[0].id + '_editor');
                
                $('#' + control[0].id + '_usingsedit')[0].value = me.htmlEncode(me.modifyUsingString((control[0].code.usings)));

                $('#' + control[0].id + '_usings').accordion({
                    collapsible: true,
                    active: false,
                    heightStyle: "content scrolling"
                });

                if (WorkflowDesignerConstants.isjava){
                    showUsings.hide();
                    $('#' + control[0].id + '_usings').hide();
                }

                var typevalue = $('#' + me.generateid('Type', prefix))[0].value.toLowerCase();
                var namevalue = $('#' + me.generateid('Name', prefix))[0].value;
                var isAsyncValue = $('#' + me.generateid('IsAsync', prefix))[0].checked;

                if (namevalue === '')
                    namevalue = '???';
                else
                    namevalue = "<b>" + namevalue + "</b>";

                var functionupper = '{';
                if (WorkflowDesignerConstants.isjava) {
                    if (typevalue === 'action') {
                        functionupper = 'function ' +
                            namevalue +
                            '(processInstance, runtime, parameter) {';
                    }
                    if (typevalue === 'condition') {
                        functionupper = 'function ' +
                            namevalue +
                            '(processInstance, runtime, parameter) {';
                    }
                    if (typevalue === 'ruleget') {
                        functionupper = 'function ' +
                            namevalue +
                            '(processInstance, runtime, parameter) {';
                    }

                    if (typevalue === 'rulecheck') {
                        functionupper = 'function ' +
                            namevalue +
                            '(processInstance, runtime, identityId, parameter) {';
                    }
                }
                else {
                    if (typevalue === 'action') {
                        var returnsvoid = isAsyncValue ? 'async Task ' : 'void ';
                        var actionparameters = isAsyncValue
                            ? ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, CancellationToken token) {'
                            : ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter) {';
                        functionupper = returnsvoid + namevalue + actionparameters;
                    }
                    if (typevalue === 'condition') {
                        var returnsbool = isAsyncValue ? 'async Task&lt;bool&gt; ' : 'bool ';
                        var conditionparameters = isAsyncValue
                            ? ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, CancellationToken token) {'
                            : ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter) {';
                        functionupper = returnsbool + namevalue + conditionparameters;
                    }
                    if (typevalue === 'ruleget') {
                        functionupper = 'IEnumerable&lt;string&gt; ' +
                            namevalue +
                            ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter) {';
                    }

                    if (typevalue === 'rulecheck') {
                        functionupper = 'bool ' +
                            namevalue +
                            ' (ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter) {';
                    }
                }


                $('#' + control[0].id + '_function_upper').html(functionupper);

                var editor = ace.edit(control[0].id + '_editor');
                editor.getSession().setMode(WorkflowDesignerConstants.isjava ? "ace/mode/javascript" :  "ace/mode/csharp");
                editor.setValue(control[0].code.code);
                editor.clearSelection();
                if (me.isReadOnly()) {
                    editor.setOptions({ readOnly: true });
                } else {
                    editor.setOptions({ readOnly: false });
                    editor.focus();
                }

                WorkflowDesignerCommon.modal(me.window, "hide");
                WorkflowDesignerCommon.modal(form, "show");
            });

        return control;
    };

    this.addAutoComplete = function (p, control) {
        if (p.datasource != undefined) {

            var source;
            if(Array.isArray(p.datasource)){
                source = function(term, suggest){
                    term = term.toLowerCase();
                    var choices = p.datasource;
                    var matches = [];
                    for (i=0; i<choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) matches.push(choices[i]);
                    suggest(matches);
                };
            }
            else {
                source = p.datasource;
            }

            var acParams = {
                minChars: 0,
                source: source,
                menuClass: 'WorkflowDesignerDialogautoComplete',
                element: control
            };

            control.autoComplete(acParams);
        }
    }

    this.modifyUsingString = function (usings) {
        var lastsymbol = usings.substring(usings.length - 1);

        if (lastsymbol === ';')
            usings = usings.substring(0, usings.length - 1);

        return usings.split(';').join(';\r\n') + ';';
    }

    this.getEasyControlValue = function (p) {
        var me = this;
        if (p.type == 'input') {
            return p.control.value;
        }
        else if (p.type == 'json') {
            return p.control.value;
        }
        else if (p.type == 'code') {
            return p.control.code;
        }
        else if (p.type == 'checkbox') {
            if(p.control.localName == "div"){
                return p.control.children[0].checked;
            }
            return p.control.checked;
        }
        else if (p.type == 'select') {
            return p.control.value;
        }
        else if (p.type == 'textarea') {
            return p.control.value;
        }
    };

    this.generateid = function (name, prefix) {
        if (prefix)
            return name + '_' + prefix + '_' + this.id;
        else
            return name + '_' + this.id;
    };

    this.GetValueByPropertyName = function (item, propertyName) {
        if (item == undefined)
            return undefined;

        if (propertyName.indexOf('.') < 0) {
            return item[propertyName];
        }
        else {
            var currValue = item;
            propertyName.split('.').forEach(function (p) {
                if (currValue != undefined)
                    currValue = currValue[p];
            });
            return currValue;
        }
    };

    this.SetValueByPropertyName = function (item, propertyName, value) {
        if (propertyName.indexOf('.') < 0) {
            return item[propertyName] = value;
        }
        else {
            var currValue = item;
            var tmp = propertyName.split('.');
            for (var i = 0; i < tmp.length; i++) {
                var p = tmp[i];
                if (i == tmp.length - 1)
                    currValue[p] = value;
                else {
                    if (currValue[p] == undefined)
                        currValue[p] = {};
                    currValue = currValue[p];
                }
            }
        }
    };

    this.ClearError = function () {
        var controls = this.window.find('.field-validation-error');
        controls.attr('title', '');
        controls.removeClass('field-validation-error');
    };

    this.ControlAddError = function (control, msg) {
        var c = $(control);
        c.addClass('field-validation-error');
        c.attr('title', msg);
    };
    this.CheckRequired = function (items, properties, msg) {
        var me = this;
        var isSuccess = true;
        items.forEach(function (item) {
            properties.forEach(function (p) {
                if (me.GetValueByPropertyName(item, p) == '') {
                    me.ControlAddError(item['control_' + p], msg);
                    isSuccess = false;
                }
            });
        });

        return isSuccess;
    };

    this.CheckUnique = function (items, properties, msg) {
        var me = this;
        var isSuccess = true;

        for (var i = 0; i < items.length; i++) {
            for (var j = i + 1; j < items.length; j++) {
                if (this._checkUniqueEquals(items[i], items[j], properties)) {
                    properties.forEach(function (p) {
                        me.ControlAddError(items[i]['control_' + p], msg);
                        me.ControlAddError(items[j]['control_' + p], msg);
                    });
                    isSuccess = false;
                }
            }
        }

        return isSuccess;
    };
    this._checkUniqueEquals = function (a, b, properties) {
        for (var i = 0; i < properties.length; i++) {
            var p = properties[i];
            if (a[p] != b[p]) {
                return false;
            }
        }
        return true;
    };
    var me = this;

    this.ClearTempField = function (data, elements) {
        if (data == undefined)
            return;

        if (elements == undefined)
            elements = this.parameters.elements;

        elements.forEach(function (e) {
            if ($.isArray(data)) {
                data.forEach(function (item) {
                    me.ClearTempField(item, elements);
                });
            }
            else {
                if (data['control_' + e.field] != undefined)
                    data['control_' + e.field] = undefined;
            }

            if (e.elements) {
                me.ClearTempField(data[e.field], e.elements);
            }
        });
    };

    this.getElementCode = function (element) {
        if (element.code != undefined)
            return element.code;
        return element.field;
    };

    this.htmlEncode = function (value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    };

    this.htmlDecode = function (value) {
        return $('<div/>').html(value).text();
    };

    this.toCompactJSON = function (value) {
        try {
            return JSON.stringify(JSON.parse(value));
        }
        catch (err) {
            try {
                return JSON5.stringify(JSON5.parse(value));
            }
            catch (err) {
                return value;
            }
        }
    };

    this.toPrettyJSON = function (value) {
        try {
            return JSON.stringify(JSON.parse(value), null, '\t');
        }
        catch (err) {
            try {
                return JSON5.stringify(JSON5.parse(value), null, '\t');
            }
            catch (err) {
                return value;
            }
        }
    }

    this.objAssign = function(target, varArgs) { // .length of function is 2
        'use strict';
        if (target == null) { // TypeError if undefined or null
          throw new TypeError('Cannot convert undefined or null to object');
        }
    
        var to = Object(target);
    
        for (var index = 1; index < arguments.length; index++) {
          var nextSource = arguments[index];
    
          if (nextSource != null) { // Skip over if undefined or null
            for (var nextKey in nextSource) {
              // Avoid bugs when hasOwnProperty is shadowed
              if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
                to[nextKey] = nextSource[nextKey];
              }
            }
          }
        }
        return to;
      };
}