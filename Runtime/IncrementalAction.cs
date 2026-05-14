using System;

namespace CLabs.Utility
{
    public class IncrementalAction
    {
        public IncrementalAction(int count, Action action = null)
        {
            if (action == null)
                Action = () => { };
            
            if(count == 0)
            {
                Action?.Invoke();
                return;
            }

            Count = count;
            Action = action;
        }
        
        public int Count { get; private set; }
        public Action Action { get; }

        public void Decrement()
        {
            if (--Count > 0) 
                return;
            
            Action?.Invoke();
        }
    }
}