﻿#region license
// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Xml.Linq;
using NUnit.Framework;
using SolrNet.Impl.FieldParsers;

namespace SolrNet.Tests {
    [TestFixture]
    public class DefaultFieldParserTests {
        [Test]
        [TestCase("str")]
        [TestCase("bool")]
        [TestCase("int")]
        [TestCase("date")]
        public void CanHandleSolrTypes(string solrType) {
            var p = new DefaultFieldParser();
            Assert.IsTrue(p.CanHandleSolrType(solrType));
        }

        [Test]
        [TestCase(typeof(float))]
        [TestCase(typeof(float?))]
        [TestCase(typeof(double))]
        [TestCase(typeof(double?))]
        [TestCase(typeof(string))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(DateTime?))]
        [TestCase(typeof(DateTimeOffset))]
        [TestCase(typeof(DateTimeOffset?))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(bool?))]
        [TestCase(typeof(Money))]
        [TestCase(typeof(Location))]
        public void CanHandleType(Type t) {
            var p = new DefaultFieldParser();
            Assert.IsTrue(p.CanHandleType(t));
        }

        [Test]
        public void ParseNullableInt() {
            var doc = new XDocument();
            doc.Add(new XElement("int", "31"));
            var p = new DefaultFieldParser();
            var i = p.Parse(doc.Root, typeof (int?));
            Assert.IsInstanceOfType(typeof(int?), i);
            var ii = (int?) i;
            Assert.IsTrue(ii.HasValue);
            Assert.AreEqual(31, ii.Value);
        }

        [Test]
        public void ParseLocation() {
            var doc = new XDocument();
            doc.Add(new XElement("str", "31.2,-44.2"));
            var p = new DefaultFieldParser();
            var l = p.Parse(doc.Root, typeof(Location));
            Assert.IsInstanceOf<Location>(l);
        }
    }
}