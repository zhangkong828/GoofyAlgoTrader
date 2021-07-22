using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Core.Tracker
{
    public class TickTracker : IObservable<Tick>
    {
        private List<IObserver<Tick>> _observers;

        public TickTracker()
        {
            _observers = new List<IObserver<Tick>>();
        }

        public IDisposable Subscribe(IObserver<Tick> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        // 用于取消订阅通知的IDisposable对象的实现
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Tick>> _observers;
            private IObserver<Tick> _observer;

            public Unsubscriber(List<IObserver<Tick>> observers, IObserver<Tick> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        private void Notify(Tick data)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(data);
            }
        }

        public void ReciveNewData(Tick data)
        {
            Notify(data);
        }
    }
}
