(function () {

    var hrefParts = window.location.href.split('/');
    var hrefPartsAllButLast = hrefParts.slice(0, -2);
    var hrefToApiGateway = hrefPartsAllButLast.join('/');

    // Add links to configuration API section
    addApiLink('#getCurrentConfig', '/api/Configuration/GetCurrentConfiguration');
    addApiLink('#overrideCurrentConfig', '/api/Configuration/OverrideCurrentConfiguration');
    addApiLink('#addServiceInstance', '/api/Configuration/AddServiceInstance');
    addApiLink('#removeServiceInstance', '/api/Configuration/RemoveServiceInstance');
    addApiLink('#updateServiceInstance', '/api/Configuration/UpdateServiceInstance');

    // Add links to examples section
    addExample('/countries');
    addExample('/usa_states');
    
    function addApiLink(aTagSelector, requestPath) {
        var apiLink = hrefToApiGateway + requestPath;
        $(aTagSelector).attr('href', apiLink);
        $(aTagSelector).attr('target', '_blank');
        $(aTagSelector).text(apiLink);
    }

    function addExample(requestPath) {
        var hostAndRequestPath = hrefToApiGateway + requestPath;
        $('#examples_ul').append('<li><a href="' + hostAndRequestPath + '" target="_blank">' + hostAndRequestPath + '</a></li>');
    }
})();