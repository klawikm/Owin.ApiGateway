(function () {

    var hrefParts = window.location.href.split('/');
    var hrefPartsAllButLast = hrefParts.slice(0, -2);
    var hrefToApiGateway = hrefPartsAllButLast.join('/');

    addExample('/countries');
    addExample('/usa_states');

    function addExample(requestPath) {
        var hostAndRequestPath = hrefToApiGateway + requestPath;
        $("#examples_ul").append('<li><a href="' + hostAndRequestPath + '">' + hostAndRequestPath + '</a></li>');
    }
})();