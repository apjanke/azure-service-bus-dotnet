﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Management
{
    using System;
    using System.Xml.Linq;

    internal static class SubscriptionRuntimeInfoExtensions
    {
        public static SubscriptionRuntimeInfo ParseFromContent(string topicName, string xml)
        {
            var xDoc = XElement.Parse(xml);
            if (!xDoc.IsEmpty)
            {
                if (xDoc.Name.LocalName == "entry")
                {
                    return ParseFromEntryElement(topicName, xDoc);
                }
            }

            throw new MessagingEntityNotFoundException("Subscription was not found");
        }

        static SubscriptionRuntimeInfo ParseFromEntryElement(string topicName, XElement xEntry)
        {
            try
            {
                var name = xEntry.Element(XName.Get("title", ManagementClientConstants.AtomNs)).Value;
                var subscriptionRuntimeInfo = new SubscriptionRuntimeInfo(topicName, name);

                var qdXml = xEntry.Element(XName.Get("content", ManagementClientConstants.AtomNs))?
                    .Element(XName.Get("SubscriptionDescription", ManagementClientConstants.SbNs));

                if (qdXml == null)
                {
                    throw new MessagingEntityNotFoundException("Subscription was not found");
                }

                foreach (var element in qdXml.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "AccessedAt":
                            subscriptionRuntimeInfo.AccessedAt = DateTime.Parse(element.Value);
                            break;
                        case "CreatedAt":
                            subscriptionRuntimeInfo.CreatedAt = DateTime.Parse(element.Value);
                            break;
                        case "UpdatedAt":
                            subscriptionRuntimeInfo.UpdatedAt = DateTime.Parse(element.Value);
                            break;
                        case "MessageCount":
                            subscriptionRuntimeInfo.MessageCount = long.Parse(element.Value);
                            break;
                        case "CountDetails":
                            subscriptionRuntimeInfo.MessageCountDetails = new MessageCountDetails();
                            foreach (var countElement in element.Elements())
                            {
                                switch (countElement.Name.LocalName)
                                {
                                    case "ActiveMessageCount":
                                        subscriptionRuntimeInfo.MessageCountDetails.ActiveMessageCount = long.Parse(countElement.Value);
                                        break;
                                    case "DeadLetterMessageCount":
                                        subscriptionRuntimeInfo.MessageCountDetails.DeadLetterMessageCount = long.Parse(countElement.Value);
                                        break;
                                    case "ScheduledMessageCount":
                                        subscriptionRuntimeInfo.MessageCountDetails.ScheduledMessageCount = long.Parse(countElement.Value);
                                        break;
                                    case "TransferMessageCount":
                                        subscriptionRuntimeInfo.MessageCountDetails.TransferMessageCount = long.Parse(countElement.Value);
                                        break;
                                    case "TransferDeadLetterMessageCount":
                                        subscriptionRuntimeInfo.MessageCountDetails.TransferDeadLetterMessageCount = long.Parse(countElement.Value);
                                        break;
                                }
                            }
                            break;
                    }
                }

                return subscriptionRuntimeInfo;
            }
            catch (Exception ex) when (!(ex is ServiceBusException))
            {
                throw new ServiceBusException(false, ex);
            }
        }
    }
}