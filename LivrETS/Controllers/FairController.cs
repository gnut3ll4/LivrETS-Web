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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LivrETS.ViewModels;
using LivrETS.Repositories;
using LivrETS.Models;
using System.Net;

namespace LivrETS.Controllers
{
    [Authorize(Roles = "Administrator,Clerk")]
    public class FairController : Controller
    {
        private const int NUMBER_OF_STEPS = 2;
        private const string CURRENT_STEP = "CurrentPhase";
        private const string SELLER = "Seller";

        private LivrETSRepository _repository;
        public LivrETSRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new LivrETSRepository();

                return _repository;
            }
        }

        // GET: /Fair/Pick
        [HttpGet]
        public ActionResult Pick()
        {
            var model = new FairViewModel();
            var currentFair = Repository.GetCurrentFair();
            var currentStep = 0;

            model.Fair = currentFair;
            model.CurrentStep = currentStep;
            model.NumberOfPhases = NUMBER_OF_STEPS;
            Session[CURRENT_STEP] = currentStep;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pick(FairViewModel model)
        {
            if (Session[CURRENT_STEP] == null)
                return RedirectToAction(nameof(Pick));

            if (ModelState.IsValid)
            {
                Fair currentFair = Repository.GetCurrentFair();
                int currentStep = (int)Session[CURRENT_STEP];

                model.Fair = currentFair;
                model.NumberOfPhases = NUMBER_OF_STEPS;

                switch (currentStep)
                {
                    case 0:
                        ApplicationUser seller = Repository.GetUserBy(BarCode: model.UserBarCode.Trim().ToUpper());

                        if (seller == null) return RedirectToAction(nameof(Pick));

                        Session[CURRENT_STEP] = 1;
                        Session[SELLER] = seller.Id;
                        model.CurrentStep = 1;
                        model.User = seller;
                        model.UserOffers = seller.Offers;
                        model.FairOffers = currentFair.Offers;
                        break;

                    case 1:
                        if (Session[SELLER] == null) return RedirectToAction(nameof(Pick));
                        ApplicationUser seller1 = Repository.GetUserBy(Id: Session[SELLER] as string);
                        IEnumerable<Offer> sellerPickedOffers = 
                            currentFair.Offers.Intersect(seller1.Offers)
                            .Where(offer => offer.Article.FairState == ArticleFairState.PICKED);

                        Session[CURRENT_STEP] = 2;
                        model.CurrentStep = 2;
                        model.User = seller1;
                        break;

                    case 2:
                        return RedirectToAction(nameof(Pick));
                }
            }

            
            return View(model);
        }

        #region Ajax

        [HttpPost]
        public ActionResult MarkAsPicked(string ArticleId)
        {
            Article article = Repository.GetArticleBy(Id: ArticleId);

            if (article == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            article.MarkAsPicked();
            Repository.Update();
            return Json(new { }, contentType: "application/json");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository?.Dispose();
                _repository = null;
            }

            base.Dispose(disposing);
        }
    }
}