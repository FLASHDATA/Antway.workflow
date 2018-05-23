/**
 * <script type="text/javascript" src="../../Scripts/WFED/include-wfed.js?local=en"></script>
 */
(function () {
    function getQueryParam(name) {
        var regex = RegExp('[?&]' + name + '=([^&]*)');

        var match = regex.exec(location.search) || regex.exec(path);
        return match && decodeURIComponent(match[1]);
    }

    function hasOption(opt, queryString) {
        var s = queryString || location.search;
        var re = new RegExp('(?:^|[&?])' + opt + '(?:[=]([^&]*))?(?:$|[&])', 'i');
        var m = re.exec(s);

        return m ? (m[1] === undefined || m[1] === '' ? true : m[1]) : false;
    }

    function getCookieValue(name) {
        var cookies = document.cookie.split('; '),
            i = cookies.length,
            cookie, value;

        while (i--) {
            cookie = cookies[i].split('=');
            if (cookie[0] === name) {
                value = cookie[1];
            }
        }

        return value;
    }

    var scriptEls = document.getElementsByTagName('script'),
        path = scriptEls[scriptEls.length - 1].src,
        local = getQueryParam('local');
    
    path = path.substring(0, path.lastIndexOf('/'));
  
    document.write('<script type="text/javascript" src="' + path + '/DesignerConstants.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Designer.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/ActivityManager.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/ActivityControl.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Background.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Keyboard.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Toolbar.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Tooltip.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/TransitionManager.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/TransitionControl.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Form.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Bar.js"></script>');
    document.write('<script type="text/javascript" src="' + path + '/Graph.js"></script>');

    document.write('<script type="text/javascript" src="' + path + '/semantic-modal.js"></script>');

    if (local != undefined && local != '') {
        document.write('<script type="text/javascript" src="' + path + '/locale/wfed-lang-' + local + '.js"></script>');
    }
    
})();