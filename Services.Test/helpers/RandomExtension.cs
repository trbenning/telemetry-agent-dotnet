// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;

namespace Services.Test.helpers
{
    public static class RandomExtension
    {
        public static string NextString(this Random rand, int length = 32, string characters = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            var builder = new StringBuilder();

            while (builder.Length < length)
            {
                builder.Append(characters[rand.Next(0, characters.Length)]);
            }

            return builder.ToString();
        }

        public static DateTimeOffset NextDateTimeOffset(this Random rand)
        {
            var min = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var max = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);

            return min + TimeSpan.FromSeconds(rand.Next(0, (int)(max - min).TotalSeconds));
        }

        public static object NextRandomTypeObject(this Random rand)
        {
            switch (rand.Next() % 3)
            {
                case 0: return rand.NextString();
                case 1: return rand.Next();
                default: return rand.Next() % 2 == 0;
            }
        }

        public static RawMessage NextRawMessage(this Random rand)
        {
            var message = new RawMessage
            {
                Id = rand.NextString(),
                DeviceId = rand.NextString(),
                Schema = rand.NextString(),
                CreateTime = rand.Next(),
                ReceivedTime = rand.Next()
            };

            foreach (var unused in Enumerable.Range(0, rand.Next(3, 10)))
            {
                message.PropsData.Add(rand.NextString(), rand.NextString());
            }

            foreach (var unused in Enumerable.Range(0, rand.Next(3, 10)))
            {
                message.PayloadData.Add(rand.NextString(), rand.NextRandomTypeObject());
            }

            return message;
        }

        public static string NextString(this Random rand, IEnumerable<string> strings)
        {
            var index = rand.Next(0, strings.Count());
            return strings.ElementAt(index);
        }
    }
}
