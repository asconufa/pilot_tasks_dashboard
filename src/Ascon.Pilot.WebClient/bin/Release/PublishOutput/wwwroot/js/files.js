var treeControl;
var treeData;

$(document).ready(function () {
    $(function() {
        $('[data-toggle="tooltip"]').tooltip();
    });

    treeControl = createTreeView(treeData);
    setObjectIdsCheckCallback();
    
    $("#sidePanel").on("hidden.bs.collapse", function () {
        $("#filesPanelContainer").removeClass("col-md-8").addClass("col-md-12");
    });
    $("#sidePanel").on("show.bs.collapse", function () {
        $("#filesPanelContainer").removeClass("col-md-12").addClass("col-md-8");
    });
    $("#renameModal").on("show.bs.modal", function() {
        var activeFileCard = $(".file-card.active");
        var id = activeFileCard.data("id");
        var name = activeFileCard.data("name");
        $("#idToRename").val(id);
        $("#oldName").val(name);
        $("#renameRootId").val(currentFolderId);
    });
    $("#removeModal").on("show.bs.modal", function() {
        var activeFileCard = $(".file-card.active");
        var id = activeFileCard.data("id");
        var name = activeFileCard.data("name");
        $("#idToRemove").val(id);
        $("#removeNameHolder").text(name);
        $("#removeRootId").val(currentFolderId);
    });
    $("#uploadModal").on("show.bs.modal", function () {
        $("#uploadRootId").val(currentFolderId);
        $("#folderId").val(currentFolderId);
    });
});

function processCardClick(el) {
    $(".file-card").removeClass("active");
    var card = $(el);
    card.addClass("active");    

    var id = card.data("id");
    var name = card.data("name");
    var size = card.data("size");
    var ext = card.data("ext");
    if (size === undefined)
    {
        $("#downloadButton").hide();
        return;
    }       

    var query = jQuery.param({
        id: id,
        name: name.endsWith(ext) ? name : name + ext,
        size: size
    });
    var downloadButton = $("#downloadButton");
    downloadButton.prop("href", downloadUrl + "?" + query);
    downloadButton.show();    
}

function processFileCardClick(el) {
    var fileCard = $(el).closest(".file-card");
    var id = fileCard.data("id");
    var name = fileCard.data("name");
    var size = fileCard.data("size");
    var ext = fileCard.data("ext");
    var typeid = fileCard.data("typeid");
    var query = jQuery.param({
        id: id,
        name: name + ext,
        size: size
    });
    var previewButton = $("#previewButton");
    if (ext === ".pdf" || ext === ".xps") {
        if (ext === ".pdf") {
            previewButton.prop("href", "/Files/Preview?" + query);
            previewButton.show();
        }
        var url = "/Files/Image/" + id + "?size=" + size + "&extension=" + ext;
        $("#viewModalContent").html('<img class="img-responsive center-block" src="' + url + '" alt="' + name + '"/>');
    } else {
        $("#viewModalContent").html('<img class="img-responsive center-block" src="/Home/GetTypeIcon/' + typeid + '"/>');
        previewButton.hide();
    }

    $("#viewModalInfo")
        .html(objectToDlist({
            "Название": name,
            "Размер": size + " байт"
        }));

    $("#modalDownloadButton").prop("href", downloadUrl + "?" + query);
    $("#viewModal").modal();
}

function objectToDlist(obj) {
    var html = '<h4><i class="glyphicon glyphicon-info-sign"></i>&nbsp;Информация</h4><dl>';
    $.each(obj,
        function (propName, propValue) {
            html += "<dt>" + propName + "</dt>";
            html += "<dd>" + propValue + "</dd>";
        });
    return html + "</dl>";
}

function downloadArchive(el) {
    $("form#downloadArchiveForm").submit();
}

function setObjectIdsCheckCallback() {
    $('input[name="objectsIds"]')
        .on("click",
            function () {
                var btn = $("#downloadArchiveButton");
                if ($('input[name="objectsIds"]:checked').length === 0)
                    btn.hide();
                else
                    btn.show();
            });
}

var recursiveFind = function (keyObj, tData) {
    var p, key, val, tRet;
    for (p in keyObj) {
        if (keyObj.hasOwnProperty(p)) {
            key = p;
            val = keyObj[p];
        }
    }
    for (p in tData) {
        if (tData.hasOwnProperty(p)) {
            if (p === key) {
                if (tData[p] === val) {
                    return tData;
                }
            } else if (tData[p] instanceof Object) {
                if (tData.hasOwnProperty(p)) {
                    tRet = recursiveFind(keyObj, tData[p]);
                    if (tRet) {
                        return tRet;
                    }
                }
            }
        }
    }
    return false;
};

function pushHistory(id) {
    history.pushState(null, "", baseFilesUrl + id);
    document.title = $("#breadcrumbs li:last-child").text();
}

function createTreeView(data) {
    var tree = $("#tree");
    tree.treeview({
        data: data,
        showIcon: true,
        showTags: true,
        onNodeSelected: function (event, node) {
            window.recieveFiles(node);
        },
        onNodeExpanded: function (event, node) {
            var childNodes = node["nodes"];
            if (childNodes.length === 0)
                window.getChilds(node);
        }
    });
    return tree.treeview(true);
}

function getChilds(node) {
    var idWithChilds = node["id"];

    /*$.each($('#tree ul li'), function (i, e) {
        var el = $(e);
        if (el.data("nodeid") === node['nodeId'])
            el.append('<span class="glyphicon glyphicon-refresh glyphicon-spin pull-right"></span>');
    });*/

    $.ajax(getChildsUrl,
    {
        data: {
            id: idWithChilds
        },
        beforeSend: function () {
            $("#sidePanelProgress").show();
        },
        success: function (data) {
            var nodeToAppendChilds = recursiveFind({ id: idWithChilds }, treeData);
            nodeToAppendChilds["nodes"] = data;

            var expandedNodes = treeControl.getExpanded();
            var selectedNode = treeControl.getSelected()[0];
            treeControl.remove();
            treeControl = createTreeView(treeData);

            //var nodeId = node['nodeId'];
            //treeControl.revealNode(nodeId, { silent: true });
            $.each(expandedNodes,
                function (i, e) {
                    treeControl.expandNode(e.nodeId, { silent: true });
                });
            //treeControl.unselectNode(treeControl.getSelected());
            //treeControl.selectNode(selectedNode);
        },
        complete: function () {
            $("#sidePanelProgress").hide();
        }
    });
}

function recieveFiles(node) {
    var folderId = node["id"];
    var filesPanel = $("#filesPanel");
    $.ajax(getFilesUrl,
    {
        data: {
            id: folderId
        },
        beforeSend: function () {
            $("#downloadArchiveButton").hide();
            filesPanel
                .html('<div class="text-center"><i style="font-size: 2em" class="glyphicon glyphicon-spin glyphicon-refresh"></i><br>загрузка</div>');
        },
        success: function (data) {
            filesPanel.html(data);
            pushHistory(folderId);
            $("#breadcrumbs").html(createHtmlForBreadcrumbs(treeControl.getSelected()[0]));
            setObjectIdsCheckCallback();
            currentFolderId = folderId;
        },
        error: function (data) {
            filesPanel.html('<div class="alert alert-danger"><p>при запросе файлов произошла ошибка</p></div>');
        }
    });
}

function createHtmlForBreadcrumbs(selectedNode) {
    function getBreadcrumbs(childNode, lBreadcrumbs) {
        var parent = treeControl.getParent(childNode);
        if (typeof parent != "undefined" && parent != null) {
            lBreadcrumbs.push(parent);
            return getBreadcrumbs(parent, lBreadcrumbs);
        } else {
            return lBreadcrumbs;
        }
    }

    var breadcrumbs = [selectedNode];
    breadcrumbs = getBreadcrumbs(selectedNode, breadcrumbs);
    var html = "";
    for (var i = breadcrumbs.length - 1; i >= 0; i--) {
        if (i === 0)
            html += "<li>" + breadcrumbs[i].text + "</li>";
        else
            html += '<li class="active"><a data-toggle="tooltip" data-placement="auto left" title="' + breadcrumbs[i].text + '" href="' + baseFilesUrl + breadcrumbs[i].id + '">' + breadcrumbs[i].text + "</a></li>";
    }
    return html;
}