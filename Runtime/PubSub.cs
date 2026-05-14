using System;
using System.Collections.Generic;

namespace CLabs.Utility {
    public delegate void EventMessage<T>(in T message) where T : struct;

    public interface IEventReceiver<TKey> {
        public (TKey, Type) Key { get; }
        public Delegate Delegate { get; }
    }
    
    public class EventReceiver<TKey, TData> : IEventReceiver<TKey> where TData : struct {
        public EventReceiver((TKey, Type) key, EventMessage<TData> action) {
            Key = key;
            Delegate = action;
        }
        
        public (TKey, Type) Key { get; }
        public Delegate Delegate { get; }
    }
    
    public interface IEventService<TKey> {
        IDisposable Subscribe(IEventReceiver<TKey>[] receivers);
        void Publish<T>((TKey, Type) key, in T message) where T : struct;
    }
    
    public class EventService<TKey> : IEventService<TKey> {
        private readonly Dictionary<(TKey, Type), List<Delegate>> m_Receivers = new();

        public IDisposable Subscribe(IEventReceiver<TKey>[] receivers) {
            foreach (var receiver in receivers) {
                if(m_Receivers.ContainsKey(receiver.Key) == false)
                    m_Receivers[receiver.Key] = new List<Delegate>();
                
                m_Receivers[receiver.Key].Add(receiver.Delegate);
            }

            return new Subscription(this, receivers);
        }

        public void Publish<T>((TKey, Type) key, in T message) where T : struct {
            if (m_Receivers.TryGetValue(key, out var receivers)) {
                foreach (var receiver in receivers.ToArray()) ((EventMessage<T>)receiver)?.Invoke(in message);
            }
        }

        private sealed class Subscription : IDisposable {
            private readonly EventService<TKey> m_Director;
            private readonly IEventReceiver<TKey>[] m_Receivers;

            public Subscription(EventService<TKey> director, IEventReceiver<TKey>[] receivers) {
                m_Director = director;
                m_Receivers = receivers;
            }

            public void Dispose() {
                foreach (var receiver in m_Receivers) {
                    if (m_Director.m_Receivers.TryGetValue(receiver.Key, out var directorReceiver)) {
                        directorReceiver.Remove(receiver.Delegate);
                    }
                }
            }
        }
    }
    
    public interface IEventPublisher<in TKey> {
        void Publish<T>(TKey key, in T message) where T : struct;
    }
    
    public readonly struct EventPublisher<TKey> : IEventPublisher<TKey> {
        private readonly IEventService<TKey> m_Service;

        public EventPublisher(IEventService<TKey> service) {
            m_Service = service;
        }
        
        public void Publish<T>(TKey key, in T message) where T : struct
            => m_Service.Publish((key, typeof(T)), in message);
    }
    
    public interface IEventSubscriber<TKey> {
        IDisposable Subscribe(IEventReceiver<TKey>[] receivers);
    }
    
    public readonly struct EventSubscriber<TKey> : IEventSubscriber<TKey> {
        private readonly IEventService<TKey> m_Service;

        public EventSubscriber(IEventService<TKey> service) {
            m_Service = service;
        }
        
        public IDisposable Subscribe(IEventReceiver<TKey>[] receivers) {
            return m_Service.Subscribe(receivers);
        }
    }

    public interface IPubSubFactory<TKey> {
        public IEventPublisher<TKey> CreatePublisher();
        public IEventSubscriber<TKey> CreateSubscriber();
    }

    public class PubSubFactory<TKey> : IPubSubFactory<TKey> {
        private readonly IEventService<TKey> m_EventService;

        public PubSubFactory(IEventService<TKey> eventService) {
            m_EventService = eventService;
        }
        
        public IEventPublisher<TKey> CreatePublisher() {
            return new EventPublisher<TKey>(m_EventService);
        }

        public IEventSubscriber<TKey> CreateSubscriber() {
            return new EventSubscriber<TKey>(m_EventService);
        }
    }
}