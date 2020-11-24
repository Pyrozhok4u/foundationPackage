using System;
using Foundation.Utils.OperationUtils;

namespace Foundation.CurrencySystem
{
    public abstract class CurrencyProvider
    {
        public int TypeId { get; }
        public Currency Currency { get; }
        
        public CurrencyProviderConfig Config { get; }

        public event EventHandler<CurrencyValueChangedEventArgs> OnValueChanged;
        
        public CurrencyProvider(Currency currency, int typeId, CurrencyProviderConfig config)
        {
            Currency = currency;
            TypeId = typeId;
            Config = config;
        }

        public virtual bool Check(int value)
        {
            return Currency.Amount >= value;
        }

        public virtual void Withdraw(int value, Action<Result> callback)
        {
            if (Check(value))
            {
                int previousAmount = Currency.Amount;
                Currency.Amount -= value;
                callback?.Invoke(new Result(true, string.Empty));
                
                var args = new CurrencyValueChangedEventArgs(previousAmount, Currency.Amount);
                OnValueChanged?.Invoke(this, args);
            }
            else
            {
                callback?.Invoke(new Result(false, string.Empty));
            }
        }

        public virtual void Deposit(int value, Action<Result> callback)
        {
            int previousAmount = Currency.Amount;
            Currency.Amount += value;
            
            var args = new CurrencyValueChangedEventArgs(previousAmount, Currency.Amount);
            OnValueChanged?.Invoke(this, args);
            callback?.Invoke(new Result(true, string.Empty));
        }
    }

    public class CurrencyValueChangedEventArgs : EventArgs
    {
        public readonly int PreviousValue;
        public readonly int CurrentValue;

        public CurrencyValueChangedEventArgs(int previousValue, int currentValue)
        {
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }
}
