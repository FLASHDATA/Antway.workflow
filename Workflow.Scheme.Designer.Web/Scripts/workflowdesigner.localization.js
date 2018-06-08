﻿var WorkflowDesignerConstants = {
    ActivityColor: '#FFE59E',
    ActivityTextColor: '#2D3436',
    ActivityInitialColor: '#27AE60',
    ActivityInitialTextColor: '#FFFFFF',
    ActivityFinalColor: '#2980B9',
    ActivityFinalTextColor: '#FFFFFF',
    ActivityConditionColor: '#FFE59E',
    ActivityConditionTextColor: '#2D3436',
    ActivityShape: '#CECECE',
    SelectColor: '#F39C12',
    SelectTextColor: '#FFFFFF',
    SelectSubProcessColor: '#e3b015',
    SelectSubProcessTextColor: '#FFFFFF',
    ButtonActive: "#D4D8D9",
    BarColor: "#EDF1F2",
    BarSeparatorColor: "#D9DEE0",
    DeleteConfirm: '¿Seguro que quieres eliminar los ítems seleccionados?',
    DeleteConfirmCurrent: '¿Seguro que quieres eliminar este ítem?',
    FieldIsRequired: '¡Se requiere campo!',
    FieldMustBeUnique: '¡El campo debe ser único!',
    ButtonTextDelete: 'Borrar',
    ButtonTextCreate: 'Crear',
    ButtonTextSave: 'Guardar',
    ButtonTextYes: 'Sí',
    ButtonTextNo: 'No',
    ButtonTextCancel: 'Cancelar',
    ButtonTextClose: 'Cerrar',
    ButtonTextUndo: 'Deshacer',
    ButtonTextRedo: 'Rehacer',
    SaveConfirm: '¿Guardar cambios?',
    CloseWithoutSaving: '¿Cerrar sin guardar?',
    
    DialogConfirmText: "Pregunta",
    None: "Ninguna",
    Warning: "Advertencia",

    InfoBlockLabel:{
        Activity: 'Activities: ',
        Transition: 'Transiciones: ',
        Command: 'Comandos: ',
    },

    ActivityNamePrefix: 'Activity_',
    ActivityFormLabel: {
        Title: 'Activity',
        Name: 'Nombre',
        State: 'Estado',
        IsInitial: 'Inicial',
        IsFinal: 'Final',
        IsForSetState: 'Condición',
        IsAutoSchemeUpdate: 'Actualización de esquema automático',
        Implementation: 'Implementación',
        PreExecutionImplementation: 'PreEjecutar Implementación',
        ImpOrder: 'Orden',
        ImpAction: 'Condición',
        ImpActionParameter: 'Parámetro de acción',
        AlwaysConditionShouldBeSingle: 'La condición siempre debe ser única',
        OtherwiseConditionShouldBeSingle: 'De lo contrario, la condición debería ser única'
    },

    TransitionFormLabel: {
        Title: 'Transición',
        Name: 'Nombre',
        From: 'De activity',
        To: 'A activity',
        Classifier: 'Clasificador',
        Restrictions: 'Restricciones',
        RestrictionsType: 'Tipo',
        RestrictionsActor: 'Actor',
        Condition: 'Condición',
        ConditionType: 'Tipo',
        ConditionAction: 'Condición',
        ResultOnPreExecution: 'Resultado en preejecución',
        Trigger: 'Trigger',
        TriggerType: 'Tipo',
        TriggerCommand: 'Comando',
        TriggerTimer: 'Timer',
        ConditionActionParameter: 'Parámetro de acción',
        ConditionInversion: 'Resultado de la acción invertida',
        ConditionsConcatenationType: 'Tipo de concatenación de condiciones',
        AllowConcatenationType: 'Permitir tipo de concatenación',
        RestrictConcatenationType: 'Restringir el tipo de concatenación',
        ConditionsListShouldNotBeEmpty: 'La lista de condiciones no debe estar vacía',
        IsFork: 'Es Subproceso',
        MergeViaSetState: 'Unir subroceso a través de establecer estado',
        DisableParentStateControl: 'Deshabilitar el control del proceso principal',
        ShowConcatParameters: "Mostrar concatenación",
        HideConcatParameters: "Ocultar concatenación"
    },
    LocalizationFormLabel: {
        Title: 'Localization',
        ObjectName: 'ObjectName',
        Type: 'Type',
        IsDefault: 'IsDefault',
        Culture: 'Culture',
        Value: 'Value',
        Types: ['Command', 'State', 'Parameter'],
    },

    TimerFormLabel: {
        Title: 'Timers',
        Name: 'Name',
        Type: 'Type',
        Value: 'Value',
        Types: ['Command', 'State', 'Parameter'],
        NotOverrideIfExists : "Do not override timer if exists"
    },

    ParameterFormLabel: {
        Title: 'Parameters',
        Name: 'Name',
        Type: 'Type',
        Purpose: 'Purpose',
        Value: 'Value',
        InitialValue: 'InitialValue',
        ShowSystemParameters : 'Show system parameters'
    },

    ActorFormLabel: {
        Title: 'Actors',
        Name: 'Name',
        Rule: 'Rule',
        Value: 'Value'
    },

    CommandFormLabel: {
        Title: 'Command',
        Name: "Name",
        InputParameters: "Input Parameters",
        InputParametersName: 'Name',
        InputParametersIsRequired: 'Required',
        InputParametersParameter: 'Parameter',
        InputParametersDefaultValue: 'Default'
    },

    AdditionalParamsFormLabel: {
        Title: 'Additional Parameters',
        IsObsolete: "IsObsolete",
        DefiningParameters: 'Defining parameters',
        ProcessParameters: 'Process parameters',
        ProcessParametersName: 'Name',
        ProcessParametersValue: 'Value'
    },
    
    CodeActionsFormLabel: {
        Title: 'Códigos de acciones y condiciones',
        Name: 'Name',
        ActionCode: 'Código Accion/Condicion',
        IsGlobal: 'Is global',
        IsAsync: 'Async',
        Type: 'Type',
        GlobalDeleteMessage: "You've deleted the Global CodeAction.<br/><b>Other schemes won't be able to call this CodeAction!</b>",
        UnGlobalMessage: "You've changed the state of the global flag.<br/>There will be created a Local CodeAction based on this Global CodeAction after saving this scheme."
    },

    ToolbarLabel: {
        CreateActivity: 'Create activity',
        CopySelected: 'Copy selected',
        Undo: 'Undo',
        Redo: 'Redo',
        Move: 'Move',
        ZoomIn: 'Zoom In',
        ZoomOut: 'Zoom Out',
        ZoomPositionDefault: 'Zoom default',
        FullScreen: 'Full Screen',
        Refresh: 'Refresh',
        AutoArrangement: 'Auto arrangement',
        Actors: 'Actors',
        Commands: 'Commands',
        Parameters: 'Parameters',
        Localization: 'Localization',
        Timers: 'Timers',
        AdditionalParameters: 'Additional Parameters',
        CodeActions: 'Acciones y Condiciones',
        Info: "Extended info",
        Delete: "Delete",
        Clone: "Clone",
        Settings: "Settings",
        CreateTransition: "Create a transition",
        CreateActivityTransition: "Create an activity and a transition",
        Legend: "Legend"

    },
    ErrorActivityIsInitialCountText: "One element must be marked flag Initial",
    ErrorReadOnlySaveText: "The Designer in ReadOnly mode, you can't save it.",
    FormMaxHeight: 700,
    EditCodeSettings: {
        Height: 600,
        Width: 1000,
        CodeHeight: 390,
        MessageBoxHeight: 400,
        MessageBoxWidth: 600,
        SuccessBoxHeight: 150,
        SuccessBoxWidth: 300
    },
    EditCodeLabel: {
        Title: "Edit code",
        EditCodeButton: 'Edit code',
        Usings: 'Usings',
        Compile: "Compile",
        CompileSucceeded: "Compilation succeeded.",
        Success: "Success",
        Error: "Error",
        OK: "OK",
        ShowUsings: "Show usings",
        HideUsings: "Hide usings",
    },
    EditJSONSettings: {
        Height: 600,
        Width: 1000,
        CodeHeight: 480
    },
     EditJSONLabel: {
        Title: "Edit value in JSON",
        CreateEmptyType: "Create",
        Format: "Format"       
     },
    isjava: false,
    OverviewMap: {
        show: true,
        width: 300,
        height: 150
    },
    UndoDepth: 200,
    DefaultCulture: 'en-US'
};