namespace SeawispHunter.RolePlaying.Attributes;

/** This stat class represents the style of stat altering presented by Daniel
    Sidhion in this article[1].

    Currently this class only works with float and int but other numerical types
    can be easily added.

    [1]: https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243
*/
public class SidhionStat<T> : ModifiableValue<T> {
  public IModifiableValue<T> rawBonusesPlus = new ModifiableValue<T>();
  public IModifiableValue<T> rawBonusesMultiply = new ModifiableValue<T>() { baseValue = one };
  public IModifiableValue<T> finalBonusesPlus = new ModifiableValue<T>();
  public IModifiableValue<T> finalBonusesMultiply = new ModifiableValue<T>() { baseValue = one };
  public SidhionStat() {
    this.Add(Modifier.Plus<T,T>(rawBonusesPlus));
    this.Add(Modifier.Multiply<T,T>(rawBonusesMultiply));
    this.Add(Modifier.Plus<T,T>(finalBonusesPlus));
    this.Add(Modifier.Multiply<T,T>(finalBonusesMultiply));
    // We don't have to do this.
    // rawBonusesPlus.PropertyChanged += ModifiersChanged;
    // rawBonusesMultiply.PropertyChanged += ModifiersChanged;
    // finalBonusesPlus.PropertyChanged += ModifiersChanged;
    // finalBonusesMultiply.PropertyChanged += ModifiersChanged;
  }

  private static T one {
    get {
      switch (Type.GetTypeCode(typeof(T))) {
        case TypeCode.Single:
          return (T) (object) 1f;
        case TypeCode.Int32:
          return (T) (object) 1;
        default:
          throw new NotImplementedException($"No one for type {typeof(T)}.");
      }
    }
  }
}
