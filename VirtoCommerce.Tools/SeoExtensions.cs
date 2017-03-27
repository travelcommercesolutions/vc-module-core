﻿using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Tools.Models;

namespace VirtoCommerce.Tools
{
    public static class SeoExtensions
    {
        /// <summary>
        /// Returns SEO path if all outline items of the first outline have SEO keywords, otherwise returns default value.
        /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
        /// </summary>
        /// <param name="outlines"></param>
        /// <param name="seoLinksType"></param>
        /// <param name="catalog"></param>
        /// <param name="storeId"></param>
        /// <param name="storeDefaultLanguage"></param>
        /// <param name="language"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSeoPath(this IEnumerable<Outline> outlines, SeoLinksType seoLinksType, string catalog, string storeId, string storeDefaultLanguage, string language, string defaultValue)
        {
            var result = defaultValue;

            if (outlines != null && seoLinksType != SeoLinksType.None)
            {
                // Find any outline for catalog
                var outline = outlines.GetOutlineForCatalog(catalog);

                if (outline != null)
                {
                    var pathSegments = new List<string>();

                    if (seoLinksType == SeoLinksType.Long)
                    {
                        pathSegments.AddRange(outline.Items
                            .Where(i => i.SeoObjectType != "Catalog")
                            .Select(i => GetBestMatchingSeoKeyword(i.SeoInfos, storeId, storeDefaultLanguage, language)));
                    }
                    else if (seoLinksType == SeoLinksType.Collapsed)
                    {
                        var lastItem = outline.Items.Last();

                        // If last item is a linked category, there is no SEO path
                        if (lastItem.SeoObjectType != "Category" || lastItem.HasVirtualParent != true)
                        {
                            pathSegments.AddRange(outline.Items
                                .Where(i => i.SeoObjectType != "Catalog" && i.HasVirtualParent != true)
                                .Select(i => GetBestMatchingSeoKeyword(i.SeoInfos, storeId, storeDefaultLanguage, language)));

                            // Add product which is linked to a virtual category
                            if (lastItem.SeoObjectType != "Catalog" && lastItem.SeoObjectType != "Category" && lastItem.HasVirtualParent == true)
                            {
                                pathSegments.Add(GetBestMatchingSeoKeyword(lastItem.SeoInfos, storeId, storeDefaultLanguage, language));
                            }
                        }
                    }
                    else
                    {
                        var lastItem = outline.Items.LastOrDefault();
                        if (lastItem != null)
                        {
                            pathSegments.Add(GetBestMatchingSeoKeyword(lastItem.SeoInfos, storeId, storeDefaultLanguage, language));
                        }
                    }

                    if (pathSegments.All(s => s != null))
                    {
                        result = string.Join("/", pathSegments);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns SEO records with highest score
        /// http://docs.virtocommerce.com/display/vc2devguide/SEO
        /// </summary>
        /// <param name="seoRecords"></param>
        /// <param name="storeId"></param>
        /// <param name="storeDefaultLanguage"></param>
        /// <param name="language"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static IList<SeoInfo> GetBestMatchingSeoInfos(this IEnumerable<SeoInfo> seoRecords, string storeId, string storeDefaultLanguage, string language, string slug)
        {
            var result = new List<SeoInfo>();

            if (seoRecords != null)
            {
                var items = seoRecords
                    .Select(s =>
                    {
                        var score = 0;
                        score += s.IsActive != false ? 16 : 0;
                        if (!string.IsNullOrEmpty(slug))
                        {
                            score += slug.EqualsInvariant(s.SemanticUrl) ? 8 : 0;
                        }
                        score += storeId.EqualsInvariant(s.StoreId) ? 4 : 0;
                        score += language.Equals(s.LanguageCode) ? 2 : 0;
                        score += storeDefaultLanguage.EqualsInvariant(s.LanguageCode) ? 1 : 0;
                        return new { SeoRecord = s, Score = score };
                    })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var first = items.FirstOrDefault();
                if (first != null)
                {
                    result.AddRange(items.Where(i => i.Score == first.Score).Select(i => i.SeoRecord));
                }
            }

            return result;
        }


        private static string GetBestMatchingSeoKeyword(IEnumerable<SeoInfo> seoRecords, string storeId, string storeDefaultLanguage, string language)
        {
            string result = null;

            // Select best matching SEO by StoreId and Language
            var bestMatchedSeo = seoRecords?.GetBestMatchingSeoInfos(storeId, storeDefaultLanguage, language, null).FirstOrDefault();

            if (bestMatchedSeo != null)
            {
                result = bestMatchedSeo.SemanticUrl;
            }

            return result;
        }
    }
}
