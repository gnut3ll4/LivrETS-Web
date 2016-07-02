﻿/*
LivrETS - Centralized system that manages selling of pre-owned ETS goods.
Copyright (C) 2016  TribuTerre

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

function checkEnter(evt) {
    var evt = (evt) ? evt : ((event) ? event : null);
    var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);
    if ((evt.keyCode == 13) && (node.type == "text")) { return false; }
}

document.onkeypress = checkEnter;

function markArticleAsPicked(articleId, trTagToMove) {
    var notifyError = function (message) {
        $.notify({
            icon: "glyphicon glyphicon-remove",
            message: message
        }, {
            type: "danger"
        })
    }
    var modal = $("#pick-confirm-dialog")

    if (modal.is(":visible")) {
        modal.modal("hide")
    }

    $.ajax({
        method: "POST",
        dataType: "json",
        url: "/Fair/MarkAsPicked",
        data: { ArticleId: articleId },
        success: function () {
            trTagToMove.detach()
            $("#picked-articles-table>tbody").append(trTagToMove)
            trTagToMove.find("button[class*='pick-article-btn']").detach()
        },
        statusCode: {
            500: function () {
                notifyError("Une erreur est survenue lors du traitement de votre demande. Svp, réessayez.")
            },
            409: function () {
                notifyError("L'article en question n'existe pas dans le système. Svp, contactez un administrateur.")
            }
        }
    })
}

$(document).ready(function () {
    $(".pick-article-btn").on("click", function () {
        window.ArticleId = $(this).attr("data-article-id")
        window.TrTagOfArticle = $(this).parents("tr")
        $("#pick-confirm-dialog").modal("show")
    })

    $("#btn-confirm-pick").on("click", function () {
        markArticleAsPicked(window.ArticleId, window.TrTagOfArticle)
        window.ArticleId = null
        window.TrTagOfArticle = null
    })

    $("#article-code-input").on("keyup", function (event) {
        if (event.keyCode == 13) {  // enter
            var articleCodeISBN = $(this).val()
            var articleTr = $("#notpicked-table>tbody")
                .find(`td:contains('${articleCodeISBN}')`)
                .parent()
            var articleId = articleTr
                .find("button[class*='pick-article-btn']")
                .attr("data-article-id")

            markArticleAsPicked(articleId, articleTr)
        }
    })
})