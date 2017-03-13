﻿using System;
using System.Collections.Generic;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime;

namespace JasperBus.Tests.Stubs
{
    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels = new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri));

        public StubTransport()
        {
            ReplyChannel = Channels[new Uri("stub://replies")];
        }

        public StubChannel ReplyChannel { get; set; }


        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public string Protocol { get; } = "stub";

        public IChannel CreateChannel(ChannelNode node)
        {
            return CreateDestinationChannel(node.Uri);
        }

        public IChannel CreateDestinationChannel(Uri destination)
        {
            return Channels[destination];
        }

        public Uri ReplyUri()
        {
            return ReplyChannel.Address;
        }
    }

    public class StubChannel : IChannel
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public StubChannel(Uri address)
        {
            Address = address;
        }

        public Uri Address { get; }

        public void StartReceiving(IReceiver receiver)
        {
            Receiver = receiver;
        }

        public IReceiver Receiver { get; set; }

        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public void Send(byte[] data, Dictionary<string, string> headers)
        {
            var callback = new StubMessageCallback();
            Callbacks.Add(callback);

            Receiver.Receive(data, headers, callback);
        }
    }

    public class StubMessageCallback : IMessageCallback
    {
        public readonly IList<Envelope> Sent = new List<Envelope>();
        public readonly IList<ErrorReport> Errors = new List<ErrorReport>();

        public void MarkSuccessful()
        {
            MarkedSucessful = true;
        }

        public bool MarkedSucessful { get; set; }

        public void MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
        }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public void MoveToDelayedUntil(DateTime time)
        {
            DelayedTo = time;
        }

        public DateTime? DelayedTo { get; set; }

        public void MoveToErrors(ErrorReport report)
        {
            Errors.Add(report);
        }

        public void Requeue()
        {
            Requeued = true;
        }

        public bool Requeued { get; set; }

        public void Send(Envelope envelope)
        {
            Sent.Add(envelope);
        }

        public bool SupportsSend { get; } = true;
    }
}