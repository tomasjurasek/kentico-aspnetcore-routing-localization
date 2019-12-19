﻿using Kentico.AspNetCore.LocalizedRouting.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.AspNetCore.LocalizedRouting
{
    public partial class TranslatedService : TranslatedRouteServiceBase, ITranslatedService
    {
        public static IEnumerable<Localized> Translations = new List<Localized>();
        private IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public TranslatedService(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;

            if (!Translations.Any())
            {
                Translations = GetTranslationsData();
            }
        }

        public string Resolve(string culture, string value)
        {
            var normalizedLang = culture.ToLowerInvariant();
            var normalizedValue = value.ToLowerInvariant();
            var translation = Translations.FirstOrDefault(s => s.LocalizerRoutes.Any(w => w.Localized == value && w.Culture == normalizedLang));
            if (translation != null)
            {
                return translation.OriginalName;
            }

            return null;
        }

        public string ResolveLinks(string culture, string value)
        {
            var normalizedLang = culture.ToLowerInvariant();
            var normalizedValue = value.ToLowerInvariant();

            var translation = Translations.FirstOrDefault(s => s.OriginalName == normalizedValue);
            var translated = translation?.LocalizerRoutes.FirstOrDefault(s => s.Culture == normalizedLang);
            if (translated != null)
            {
                return translated.Localized;
            }

            return null;
        }


        protected override IEnumerable<Localized> GetTranslationsData()
        {
            var actions = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .Where(s => (s as ControllerActionDescriptor).MethodInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Any())
                .Select(s => new Localized
                {
                    OriginalName = (s as ControllerActionDescriptor).ActionName.ToLower(),
                    LocalizerRoutes = (s as ControllerActionDescriptor).MethodInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Select(c => new LocalizedRoute
                    {
                        Culture = (c as LocalizedRouteAttribute).Culture.ToLower(),
                        Localized = (c as LocalizedRouteAttribute).LocalizedRoute.ToLower()
                    }).ToList()
                });

            var controllers = _actionDescriptorCollectionProvider.ActionDescriptors.Items
              .Where(s => (s as ControllerActionDescriptor).ControllerTypeInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Any())
              .Select(s => new Localized
              {
                  OriginalName = (s as ControllerActionDescriptor).ControllerName.ToLower(),
                  LocalizerRoutes = (s as ControllerActionDescriptor).ControllerTypeInfo.GetCustomAttributes(typeof(LocalizedRouteAttribute), true).Select(c => new LocalizedRoute
                  {
                      Culture = (c as LocalizedRouteAttribute).Culture.ToLower(),
                      Localized = (c as LocalizedRouteAttribute).LocalizedRoute.ToLower()
                  }).ToList()
              }).GroupBy(s => s.OriginalName).Select(s => s.FirstOrDefault());

            return controllers.Concat(actions);
        }
    }
}