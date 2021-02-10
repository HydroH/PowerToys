﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Mono.Collections.Generic;
using NUnit.Framework;
using PowerLauncher.Plugin;
using Wox.Plugin;

namespace Wox.Test
{
    public class QueryBuilderTest
    {
        private static bool AreEqual(Query firstQuery, Query secondQuery)
        {
            // Using Ordinal since this is used internally
            return firstQuery.ActionKeyword.Equals(secondQuery.ActionKeyword, StringComparison.Ordinal)
                && firstQuery.Search.Equals(secondQuery.Search, StringComparison.Ordinal)
                && firstQuery.RawQuery.Equals(secondQuery.RawQuery, StringComparison.Ordinal);
        }

        [Test]
        public void QueryBuilderShouldRemoveExtraSpacesForNonGlobalPlugin()
        {
            // Arrange
            PluginManager.SetAllPlugins(new List<PluginPair>()
            {
                new PluginPair
                {
                    Metadata = new PluginMetadata() { ActionKeyword = ">" },
                },
            });

            string searchQuery = ">   file.txt    file2 file3";

            // Act
            var pluginQueryPairs = QueryBuilder.Build(ref searchQuery);

            // Assert
            Assert.AreEqual("> file.txt file2 file3", searchQuery);
        }

        [Test]
        public void QueryBuilderShouldRemoveExtraSpacesForDisabledNonGlobalPlugin()
        {
            // Arrange
            PluginManager.SetAllPlugins(new List<PluginPair>()
            {
                new PluginPair { Metadata = new PluginMetadata() { Disabled = true, ActionKeyword = ">" } },
            });

            string searchQuery = ">   file.txt    file2 file3";

            // Act
            var pluginQueryPairs = QueryBuilder.Build(ref searchQuery);

            // Assert
            Assert.AreEqual("> file.txt file2 file3", searchQuery);
        }

        [Test]
        public void QueryBuilderShouldRemoveExtraSpacesForGlobalPlugin()
        {
            // Arrange
            string searchQuery = "file.txt  file2  file3";

            // Act
            var pluginQueryPairs = QueryBuilder.Build(ref searchQuery);

            // Assert
            Assert.AreEqual("file.txt file2 file3", searchQuery);
        }

        [Test]
        public void QueryBuildShouldGenerateSameSearchQueryWithOrWithoutSpaceAfterActionKeyword()
        {
            // Arrange
            var plugin = new PluginPair { Metadata = new PluginMetadata() { ActionKeyword = "a" } };
            PluginManager.SetAllPlugins(new List<PluginPair>()
            {
                plugin,
            });

            var firstQueryText = "asearch";
            var secondQueryText = "a search";

            // Act
            var firstPluginQueryPair = QueryBuilder.Build(ref firstQueryText);
            var firstQuery = firstPluginQueryPair.GetValueOrDefault(plugin);

            var secondPluginQueryPairs = QueryBuilder.Build(ref secondQueryText);
            var secondQuery = secondPluginQueryPairs.GetValueOrDefault(plugin);

            // Assert
            // Using Ordinal since this is used internally
            Assert.IsTrue(firstQuery.Search.Equals(secondQuery.Search, StringComparison.Ordinal));
            Assert.IsTrue(firstQuery.ActionKeyword.Equals(secondQuery.ActionKeyword, StringComparison.Ordinal));
        }

        [Test]
        public void QueryBuildShouldGenerateCorrectQueryForPluginsWhoseActionKeywordsHaveSamePrefix()
        {
            // Arrange
            string searchQuery = "abcdefgh";
            var firstPlugin = new PluginPair { Metadata = new PluginMetadata { ActionKeyword = "ab", ID = "plugin1" } };
            var secondPlugin = new PluginPair { Metadata = new PluginMetadata { ActionKeyword = "abcd", ID = "plugin2" } };
            PluginManager.SetAllPlugins(new List<PluginPair>()
            {
                firstPlugin,
                secondPlugin,
            });

            // Act
            var pluginQueryPairs = QueryBuilder.Build(ref searchQuery);

            var firstQuery = pluginQueryPairs.GetValueOrDefault(firstPlugin);
            var secondQuery = pluginQueryPairs.GetValueOrDefault(secondPlugin);

            // Assert
            Assert.IsTrue(AreEqual(firstQuery, new Query { RawQuery = searchQuery, Search = searchQuery.Substring(firstPlugin.Metadata.ActionKeyword.Length), ActionKeyword = firstPlugin.Metadata.ActionKeyword } ));
            Assert.IsTrue(AreEqual(secondQuery, new Query { RawQuery = searchQuery, Search = searchQuery.Substring(secondPlugin.Metadata.ActionKeyword.Length), ActionKeyword = secondPlugin.Metadata.ActionKeyword }));
        }

        [Test]
        public void QueryBuilderShouldSetTermsCorrectlyWhenCalled()
        {
            // Arrange
            string searchQuery = "abcd efgh";
            var firstPlugin = new PluginPair { Metadata = new PluginMetadata { ActionKeyword = "ab", ID = "plugin1" } };
            var secondPlugin = new PluginPair { Metadata = new PluginMetadata { ActionKeyword = "abcd", ID = "plugin2" } };
            PluginManager.SetAllPlugins(new List<PluginPair>()
            {
                firstPlugin,
                secondPlugin,
            });

            // Act
            var pluginQueryPairs = QueryBuilder.Build(ref searchQuery);

            var firstQuery = pluginQueryPairs.GetValueOrDefault(firstPlugin);
            var secondQuery = pluginQueryPairs.GetValueOrDefault(secondPlugin);

            // Assert
            // Using Ordinal since this is used internally
            Assert.IsTrue(firstQuery.Terms[0].Equals("cd", StringComparison.Ordinal) && firstQuery.Terms[1].Equals("efgh", StringComparison.Ordinal) && firstQuery.Terms.Count == 2);
            Assert.IsTrue(secondQuery.Terms[0].Equals("efgh", StringComparison.Ordinal) && secondQuery.Terms.Count == 1);
        }
    }
}
