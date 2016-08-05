function includeConnection(connectionName) {
    const connDisplay = $(`#conn\\.display\\.${connectionName}`).eq(0);
    const connIncl = $(`#conn\\.incl\\.${connectionName}`).eq(0);

    if (connDisplay.hasClass("emphasis")) {
        connDisplay.removeClass("emphasis");
        connIncl.val(false);
    } else {
        connDisplay.addClass("emphasis");
        connIncl.val(true);
    }
};

$(document)
    .ready(function () {
        $("[name='my-checkbox']").bootstrapSwitch();

        Array.prototype.slice.apply(document.getElementsByClassName("listEntry"))
            .forEach(function(item) {
                item.onclick = function() {
                    const target = $(item).eq(0).attr("data-target");
                    const detailView = document.getElementById(target);

                    if (detailView.style.display === "block") {
                        detailView.style.display = "none";
                    } else {
                        detailView.style.display = "block";
                    }
                };
            });
    });