/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/
using System;
using System.ComponentModel;

namespace SeawispHunter.RolePlay.Attributes;

public class Value<T> : IValue<T> {

  protected T _value;
  public virtual T value {
    get => _value;
    set {
      _value = value;
      OnChange(nameof(value));
    }
  }
  public Value() {}
  public Value(T value) => _value = value;

  public event PropertyChangedEventHandler PropertyChanged;

  private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, name == nameof(value)
                                    ? valueEventArgs
                                    : new PropertyChangedEventArgs(name));
  }
}

public static class Value {
  public static IReadOnlyValue<T> FromFunc<T>(Func<T> f, out Action callOnChange) => new DerivedValue<T>(f, out callOnChange);

  public static IReadOnlyValue<T> FromFunc<T>(Func<T> f) => new DerivedValue<T>(f, out var callOnChange);

  public static IValue<T> FromFunc<T>(Func<T> f, Action<T> @set, out Action callOnChange)
    => new DerivedMutableValue<T>(f, @set, out callOnChange);

  public static IValue<T> WithBounds<T>(T value, T lowerBound, T upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           new ReadOnlyValue<T>(lowerBound),
                           new ReadOnlyValue<T>(upperBound));

  public static IValue<T> WithBounds<T>(T value, T lowerBound, IReadOnlyValue<T> upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           new ReadOnlyValue<T>(lowerBound),
                           upperBound);

  public static IValue<T> WithBounds<T>(T value, IReadOnlyValue<T> lowerBound, T upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           lowerBound,
                           new ReadOnlyValue<T>(upperBound));

  public static IValue<T> WithBounds<T>(T value, IReadOnlyValue<T> lowerBound, IReadOnlyValue<T> upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           lowerBound,
                           upperBound);

  internal class DerivedValue<T> : IReadOnlyValue<T> {
    private Func<T> func;

    public DerivedValue(Func<T> func, out Action callOnChange) {
      this.func = func;
      callOnChange = OnChange;
    }

    public T value => func();

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

  internal class DerivedMutableValue<T> : IValue<T> {
    private readonly Func<T> @get;
    private readonly Action<T> @set;

    public DerivedMutableValue(Func<T> @get, Action<T> @set, out Action callOnChange) {
      this.@get = @get;
      this.@set = @set;
      callOnChange = OnChange;
    }

    public T value {
      get => @get();
      set => @set(value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

  internal class BoundedValue<T> : IValue<T>
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
  {
    public readonly IReadOnlyValue<T> lowerBound;
    public readonly IReadOnlyValue<T> upperBound;
    private T _value;

    public T value {
      get => _value;
      set {
#if NET6_0_OR_GREATER
        _value = value;
        if (_value < lowerBound.value)
          _value = lowerBound.value;
        if (_value > upperBound.value)
          _value = upperBound.value;
#else
        var op = Modifier.GetOp<T>();
        _value = op.Max(lowerBound.value, op.Min(upperBound.value, value));
#endif
        OnChange();
      }
    }

    public BoundedValue(T value, IReadOnlyValue<T> lowerBound, IReadOnlyValue<T> upperBound) {
      _value = value;
      this.lowerBound = lowerBound;
      // this.lowerBound.PropertyChanged -= BoundChanged;
      this.lowerBound.PropertyChanged += BoundChanged;
      this.upperBound = upperBound;
      // this.upperBound.PropertyChanged -= BoundChanged;
      this.upperBound.PropertyChanged += BoundChanged;
    }

    protected void BoundChanged(object sender, PropertyChangedEventArgs e) {
      this.value = _value;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

}
public class ReadOnlyValue<T> : IReadOnlyValue<T> {
  public T value { get; private init; }
  public ReadOnlyValue(T value) => this.value = value;

  // We don't ever call this because we don't change.
  // This isn't strictly true unless T is a struct.
  public event PropertyChangedEventHandler PropertyChanged {
    add { }
    remove { }
  }
}
