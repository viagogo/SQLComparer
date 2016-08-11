'use strict';

function includeConnection(connectionName) {
    const connDisplay = $(`#conn\\.display\\.${connectionName}`).eq(0);
    const connIncl = $(`#conn\\.incl\\.${connectionName}`).eq(0);

    if (connDisplay.hasClass('emphasis')) {
        connDisplay.removeClass('emphasis');
        connIncl.val(false);
    } else {
        connDisplay.addClass('emphasis');
        connIncl.val(true);
    }
};

function handlePush(obj, direction) {
    const targetDb = obj.find("[name='TargetDatabase']").eq(0);
    const leftAlias = obj.find("[name='LeftAlias']").eq(0);
    const rightAlias = obj.find("[name='RightAlias']").eq(0);
    const leftEntity = obj.find("[name='LeftEntity']").eq(0);
    const rightEntity = obj.find("[name='RightEntity']").eq(0);

    const options = {
        url: '/Comparison/Push',
        type: 'post',
        data: {
            'PushFromLeftToRight': direction === 'right',
            'PushFromRightToLeft': direction === 'left',
            'TargetDatabase': targetDb.val(),
            'LeftAlias': leftAlias.val(),
            'RightAlias': rightAlias.val(),
            'LeftEntity': leftEntity.val(),
            'RightEntity': rightEntity.val()
        }
    };

    $.ajax(options)
        .done(function (data) {
            const gridView = $(obj.closest('.modal').prevAll('.gridComparison')[0]);
            if (gridView) {
                gridView.hover(function () {
                    $(this).fadeTo(500, 1);
                }, function () {
                    $(this).fadeTo(500, 0.2);
                });
            }

            // TODO:
            // Re-calculate gridview
            // 
            // Store all connection strings and identifiers in side-by-side modal
            // Call GetComparisonsBetweenIdentifiers and wrap it in a partial view
            // Replace existing grid with new one

            const title = gridView.find('h3').eq(0);
            title.html(title.html + ' -- OUT OF DATE! RELOAD THE PAGE IF YOU WANT TO SEE THE UPDATED COMPARISON');
            title.css('background-color', 'orangered');
            
            alert(data.message);
        });

    return false;
};

$(document)
    .ready(function () {
        $("[name='my-checkbox']").bootstrapSwitch();

        Array.prototype.slice.apply(document.getElementsByClassName('listEntry'))
            .forEach(function(item) {
                item.onclick = function() {
                    const target = $(item).eq(0).attr('data-target');
                    const detailView = document.getElementById(target);

                    if (detailView.style.display === 'block') {
                        detailView.style.display = 'none';
                    } else {
                        detailView.style.display = 'block';
                    }
                };
            });

        $('.pushToLeft').click(function () {
            handlePush($(this).parent(), 'left');
            return false;
        });

        $('.pushToRight').click(function () {
            handlePush($(this).parent(), 'right');
            return false;
        });
    });