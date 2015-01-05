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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Utils;

namespace SolrNet.Tests {
    public class DateTimeFieldParserTests {
        [Test, TestCaseSource("parsedDates")]
        public static void ParseYearsTest(KeyValuePair<string, DateTime> pd)
        {
            Assert.AreEqual(pd.Value, DateTimeFieldParser.ParseDate(pd.Key));
            Assert.AreEqual(pd.Value.Kind, pd.Value.Kind);
        }

        private static readonly IEnumerable<KeyValuePair<string, DateTime>> parsedDates =
            new[] {
                KV.Create("1-01-01T00:00:00Z", new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                KV.Create("2004-11-01T00:00:00Z", new DateTime(2004, 11, 1, 0, 0, 0, DateTimeKind.Utc)),
                KV.Create("2012-05-10T14:17:23.684Z", new DateTime(2012, 5, 10, 14, 17, 23, 684, DateTimeKind.Utc)),
                KV.Create("2012-05-10T14:17:23.68Z", new DateTime(2012, 5, 10, 14, 17, 23, 680, DateTimeKind.Utc)),
                KV.Create("2012-05-10T14:17:23.6Z", new DateTime(2012, 5, 10, 14, 17, 23, 600, DateTimeKind.Utc)),
            };


        [Test, TestCaseSource("dateTimes")]
        public void RoundTrip(DateTime dt)
        {

            var value = DateTimeFieldParser.ParseDate(DateTimeFieldSerializer.SerializeDate(dt));
            Assert.AreEqual(dt, value);
            Assert.AreEqual(dt.Kind, value.Kind);

        }


        [Test, TestCaseSource("dateTimes")]
        public void NullableRoundTrips(DateTime dt)
        {
            var parser = new NullableFieldParser(new DateTimeFieldParser());
            var serializer = new NullableFieldSerializer(new DateTimeFieldSerializer());

            var s = serializer.Serialize(dt).First().FieldValue;
            var xml = new XDocument();
            xml.Add(new XElement("date", s));
            var value = (DateTime?)parser.Parse(xml.Root, typeof(DateTime?));
            Assert.AreEqual(dt, value);
            Assert.AreEqual(dt.Kind, value.Value.Kind);
        }

        private static readonly IEnumerable<DateTime> dateTimes =
            new[] {
                new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2004, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2004, 11, 1, 15, 41, 23, DateTimeKind.Utc),
                new DateTime(2012, 5, 10, 14, 17, 23, 684, DateTimeKind.Utc),
                new DateTime(2008, 5, 6, 14, 21, 23, 0, DateTimeKind.Utc),
            };
    }
}