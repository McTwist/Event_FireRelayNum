# Output Event - FireRelayNum (v6)
Choose which onRelays to fire

## Description
You choose which onRelays to fire. Just insert the number, or numbers of those relays.

### Example

```
onActivate -> Self -> fireRelayNum ( 1, Brick)
onRelay -> Self -> setColor ( blue )
onRelay -> Self -> setColor ( white )
```

The brick will be colored blue.
You can also insert multiple number, like: 1 5-7 9 11 15-17
That will fire the relays on: 1, 5, 6, 7, 9, 11, 15, 16 and 17.

The list is the direction of the event where Brick is the default current brick.

```
onActivate -> Self -> fireRelayRandomNum ( "1-2", Brick )
onRelay -> Self -> setColor ( blue )
onRelay -> Self -> setColor ( white )
```

The brick will randomly become blue or white.

**Protip:** Adding the same number more times in fireRelayRandomNum results in it being picked more frequently.

*"If the event does not come to you, you come to it."*

## Installation
Put Event_FireRelayNum.zip into the Add-Ons folder in your Blockland folder.