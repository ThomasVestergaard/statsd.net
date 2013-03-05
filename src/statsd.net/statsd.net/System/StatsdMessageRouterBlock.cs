﻿using statsd.net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace statsd.net.System
{
  internal class StatsdMessageRouterBlock : ITargetBlock<StatsdMessage>
  {
    private ITargetBlock<StatsdMessage> _gauges;
    private ITargetBlock<StatsdMessage> _counters;
    private List<ITargetBlock<StatsdMessage>> _timings;

    public StatsdMessageRouterBlock()
    {
      _timings = new List<ITargetBlock<StatsdMessage>>();
    }

    public void AddTarget(MessageType message, ITargetBlock<StatsdMessage> target)
    {
      switch (message)
      {
        case MessageType.Counter: _counters = target; break;
        case MessageType.Gauge: _gauges = target; break;
        case MessageType.Timing: _timings.Add(target); break;
      }
    }
    
    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, StatsdMessage messageValue, ISourceBlock<StatsdMessage> source, bool consumeToAccept)
    {
      switch (messageValue.MessageType)
      {
        case MessageType.Counter: _counters.Post(messageValue); break;
        case MessageType.Gauge: _gauges.Post(messageValue); break;
        case MessageType.Timing: 
          if (_timings.Count == 1) 
          {
            _timings[0].Post(messageValue);
          }
          else 
          {
            _timings.ForEach(p => p.Post(messageValue));
          }
          break;
      }
      return DataflowMessageStatus.Accepted;
    }

    public void Complete()
    {
      throw new NotImplementedException();
    }

    public Task Completion
    {
      get { throw new NotImplementedException(); }
    }

    public void Fault(Exception exception)
    {
      throw new NotImplementedException();
    }
  }
}