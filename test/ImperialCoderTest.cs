
/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using SeawispHunter.RolePlay.Attributes;
namespace SeawispHunter.RolePlay.Attributes.Test {

/** This is a [forum
    post](https://gamedev.net/forums/topic/683035-designing-a-stateffect-system-for-an-rpg/5314850/)
    that specified a number of effects that I thought I'd try to capture with
    this attribute design.

    * * *

    What I would like some help with is this: How would I design/structure a
    creature stat and effect system so that it is elegant and flexible in that I
    can create effects that can be carried out and applied in several ways.

    * -15 to HP - Standard effect that desecrates a stat.

    * +25% Max Health to HP - Effect that increases a stat by a percentage of another stat.

    * 3pts Fire damage every 2 seconds for 15 seconds (Total 16 damage.) - An
      effect which decreases a stat intermittently until it has been doing so
      for a duration of time specified. (Damage types - fire, cold, poison - is
      also something I've had trouble with if anyone's feeling particularly
      generous.)

    * -2 to movement speed until removed - An effect which has a constant effect
      on a stat until the effect is removed; for example until the player has
      found a "Cure Illness" potion or removes a piece of armor which possesses
      this effect.

    Obviously I'm not just looking for the ability to create the effects
    described above, but the ability to specify a number of variables to create
    any effect with the combination of the traits of the effects described
    above.

    Basically I have no idea weather I need a single effect class or multiple
    effect classes. I don't know whether I need a class for a single entity's
    stat which may or may not have caps (HP vs something like Luck), or to store
    the stats in an Enum or simply an array of integers. Should stats have base
    and effected values? I think what I'm looking for is what
    CreatureStat/CreatureEffect classes I should be making and what
    variables/properties the CreatureEffect class(es) should have.

    (END MODERATOR EDIT)

    Basically I won't go over it all again but something I didn't mention in my
    post is that one of the big issues I'm having is creating this system so
    that everything feels neat and efficient. Sure I could (probably) code a
    solution up which would do the job but at the same time look horrific an
    preform awfully. I don't just want to implement the system, I want to
    implement the system well.

    Also something else I didn't mention was having the ability to create
    effects like this: "+10% damage against undead". This effect would add a 10%
    bonus to the creature's attack when the attack is dealt to an undead enemy.
    It might be worth mentioning that nothing is set in stone with my game's
    design right now. Enemy types, enemy variants, damage types, stats and
    abilities, these are all things that aren't solid concepts yet. I'm really
    waiting to get this system in place to move forward with the design of the
    game.

    For the record I am programming in C# using XNA, not that XNA is much
    relevance. Thank you for any feedback or future discussion. :)

    * * *

    I've written tests for each of the bullet points above, some broken into
    two. In a real scenario, I'd have a custom implementation for `IModifier<T>`
    that might have properties available on it like `curable`. Maybe a readonly
    field to hold a FightContext. All modifiers would look at one FightContext
    object. You set it that one object before looking up values.

    There would also probably be a generalized character class.

    ``` c#
    public interface ICharacter {
      IModifiableValue<float> maxHealth { get; }
      IModifiableValue<float> health { get; }
      IMutableValue<float> damage { get; }
      DamageType vulnerable;
      DamageType attacking;
    }
    ```

    That way you could write code that worked symmetrically for your `Player :
    ICharacter` and `Enemy : ICharacter`.
   */
public class ImperialCoderTest {

  ModifiableValue<float> maxHealth = new ModifiableValue<float>(100f);
  IValue<float> health;
  IModifier<float,float> boost = Modifier.Times(1.10f, "10% boost");
  IModifier<float,float> boost20 = Modifier.Times(1.2f, "20% boost");

  [Flags]
  internal enum DamageType {
    None = 0,
    Fire   = 1,
    Cold   = 2,
    Poison = 4,
    All = 7,
  }
  internal class FightContext {
    public DamageType vulnerable;
    public DamageType incoming;
    public bool targetIsUndead;
  }

  public ImperialCoderTest() {
    health = Value.WithBounds(maxHealth.value, 0, maxHealth);
  }

  [Fact] public void TestMinusHP() {
    Assert.Equal(100f, health.value);
    health.value -= 15f;
    Assert.Equal(85f, health.value);
  }

  [Fact] public void TestPlusMaxHP() {
    health.value = 50f;
    Assert.Equal(50f, health.value);
    health.value += 0.25f * maxHealth.value;
    Assert.Equal(75f, health.value);
  }

  [Fact] public void TestPlusMaxHPOverHeal() {
    Assert.Equal(100f, health.value);
    health.value += 0.25f * maxHealth.value;
    Assert.Equal(100f, health.value);
    health.value -= 15f;
    Assert.Equal(85f, health.value);
  }

  [Fact] public void TypedDamage() {
    var typedDamage = new ModifiableValue<float>();
    var fightContext = new FightContext();
    typedDamage.modifiers.Add(Modifier.FromFunc((float x) => (fightContext.vulnerable & fightContext.incoming) == 0 ? x : 2f * x));
    typedDamage.initial.value = 10f;
    Assert.Equal(10f, typedDamage.value);
    fightContext.incoming = DamageType.Fire;
    Assert.Equal(10f, typedDamage.value);
    fightContext.vulnerable = DamageType.Fire;
    Assert.Equal(20f, typedDamage.value);
    fightContext.vulnerable = DamageType.All;
    Assert.Equal(20f, typedDamage.value);
  }

  [Fact] public void UndeadDamage() {
    var typedDamage = new ModifiableValue<float>();
    var fightContext = new FightContext();
    typedDamage.modifiers.Add(Modifier.FromFunc((float x) => fightContext.targetIsUndead ? 1.1f * x : x));
    typedDamage.initial.value = 10f;
    Assert.Equal(10f, typedDamage.value);
    fightContext.targetIsUndead = true;
    Assert.Equal(11f, typedDamage.value);
  }

  [Fact] public async Task IntermittentFireDamageNotVulnerable() {
    var typedDamage = new ModifiableValue<float>();
    var fightContext = new FightContext();
    // In actuality, we'd accept a cancellation token for this effect.
    CancellationToken token = default;
    fightContext.incoming = DamageType.Fire;
    fightContext.vulnerable = DamageType.None;
    typedDamage.modifiers.Add(Modifier.FromFunc((float x) => (fightContext.vulnerable & fightContext.incoming) == 0 ? x : 2f * x));
    typedDamage.initial.value = 2f;

    Assert.Equal(100f, health.value);
    for (int i = 0; i < 8 && ! token.IsCancellationRequested; i++) {
      health.value -= typedDamage.value;
      // For unit test purposes, let's wait just 2ms not 2s.
      // await Task.Delay(TimeSpan.FromSeconds(2f), token);
      await Task.Delay(TimeSpan.FromMilliseconds(2f), token);
    }
    // Does 16 damage.
    Assert.Equal(84f, health.value);
  }

  [Fact] public async Task IntermittentFireDamageVulnerable() {
    var typedDamage = new ModifiableValue<float>();
    var fightContext = new FightContext();
    // In actuality, we'd accept a cancellation token for this effect.
    CancellationToken token = default;
    fightContext.incoming = DamageType.Fire;
    fightContext.vulnerable = DamageType.All;
    typedDamage.modifiers.Add(Modifier.FromFunc((float x) => (fightContext.vulnerable & fightContext.incoming) == 0 ? x : 2f * x));
    typedDamage.initial.value = 2f;

    Assert.Equal(100f, health.value);
    for (int i = 0; i < 8 && ! token.IsCancellationRequested; i++) {
      health.value -= typedDamage.value;
      // For unit test purposes, let's wait just 2ms not 2s.
      // await Task.Delay(TimeSpan.FromSeconds(2f), token);
      await Task.Delay(TimeSpan.FromMilliseconds(2f), token);
    }
    // Does double damage, 32 damage.
    Assert.Equal(68f, health.value);
  }

  [Fact] public void SlowMovementSpeed() {
    var speed = new ModifiableValue<int>(10);
    var ailment = Modifier.Minus(2, "ailment (curable)");
    Assert.Equal(10, speed.value);
    speed.modifiers.Add(ailment);
    Assert.Equal(8, speed.value);
    Cure(speed);
    Assert.Equal(10, speed.value);
  }

  /** Remove ailments that have "curable" in their name. Not how one would
      actually do it in practice. You'd want some thing better than string
      comparison, but that's what IModifier<T> is an interface. Add your own
      implementation with an `isCurable` property. */
  void Cure<T>(IModifiableValue<T> attr) {
    // We're using Linq here so we don't have to build up a list that we then
    // iterate through again to remove.
    foreach (var modifier in attr.modifiers
             // HACK: We shouldn't need to know this.
             .Cast<ContextModifier<T,T>>()
             .Where(m => m.name.Contains("curable"))
             .ToList())
        attr.modifiers.Remove(modifier);
  }


}
}
