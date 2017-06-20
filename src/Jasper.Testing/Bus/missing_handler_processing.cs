﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class missing_handler_processing : IntegrationContext
    {
        [Fact]
        public async Task missing_handlers_are_called()
        {
             NoMessageHandler1.Reset();
             NoMessageHandler2.Reset();

            with(r =>
            {
                r.Services.AddService<IMissingHandler, NoMessageHandler1>();
                r.Services.AddService<IMissingHandler, NoMessageHandler2>();
            });

            var msg1 = new MessageWithNoHandler();

            await Bus.Consume(msg1);

            await NoMessageHandler1.Finished;
            await NoMessageHandler2.Finished;

            NoMessageHandler1.Handled.Single().Message.ShouldBe(msg1);
            NoMessageHandler2.Handled.Single().Message.ShouldBe(msg1);
        }
    }

    public class NoMessageHandler1 : IMissingHandler
    {
        public static readonly List<Envelope> Handled = new List<Envelope>();
        private static TaskCompletionSource<Envelope> _source;

        public static void Reset()
        {
            Handled.Clear();
            _source = new TaskCompletionSource<Envelope>();
        }

        public static Task<Envelope> Finished => _source.Task;

        public void Handle(Envelope envelope)
        {
            _source.SetResult(envelope);
            Handled.Add(envelope);
        }
    }

    public class NoMessageHandler2 : IMissingHandler
    {
        public static readonly List<Envelope> Handled = new List<Envelope>();
        private static TaskCompletionSource<Envelope> _source;

        public static void Reset()
        {
            Handled.Clear();
            _source = new TaskCompletionSource<Envelope>();
        }

        public static Task<Envelope> Finished => _source.Task;

        public void Handle(Envelope envelope)
        {
            _source.SetResult(envelope);
            Handled.Add(envelope);
        }
    }

    public class MessageWithNoHandler
    {

    }
}