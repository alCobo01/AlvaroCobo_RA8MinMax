# Explicació de l'algoritme Min-Max amb poda Alpha-Beta

## Què és el Min-Max?

El Min-Max és un algoritme per jocs de dos jugadors per torns. La idea és que hi ha dos rols oposats:

- **Maximizer** (la IA): vol obtenir el valor més alt possible
- **Minimizer** (el jugador humà): vol obtenir el valor més baix possible

L'algoritme construeix un arbre on cada node és un estat del tauler i cada fill és una jugada possible. Els nivells alternen entre IA i jugador. Quan s'arriba a un estat final, s'avalua: +1 si guanya la IA, -1 si guanya el jugador, 0 si és empat. Després es propaguen els valors cap amunt: el Maximizer tria el fill amb valor més alt, el Minimizer el més baix.

```
         Torn IA (MAX)
        /      |      \
     [0,0]   [0,1]   [1,2]       ← jugades possibles
      /\      /\       /\
    ...  ... ...  ... ...  ...    ← respostes del rival
    +1  0   0   -1   0   0       ← valors terminals

  La IA tria [0,0] perquè dona +1
```

## Poda Alpha-Beta

Optimització que evita explorar branques innecessàries. Es mantenen dos valors:

- **Alpha**: millor valor garantit pel Maximizer
- **Beta**: millor valor garantit pel Minimizer

Quan `alpha >= beta`, sabem que un jugador mai triarà aquesta branca perquè ja té una opció millor, i podem deixar d'explorar-la.

## Implementació al codi

**Node.cs** - Cada node guarda: l'estat del tauler (`MatrixNode`), quin equip juga (`Team`: +1 o -1), els valors `Alpha`/`Beta`, el `Value` calculat, els fills (`NodeChildren`) i el millor fill trobat (`BestChild`). El `Value` s'inicialitza a `-team * 2` perquè sigui pitjor que qualsevol resultat real.

**GameManager.cs** - `EnemyResponse()` clona el tauler, crea el node arrel i crida `MinMax()`. Després agafa `BestChild` de l'arrel i col·loca la fitxa. El mètode `MinMax()` és recursiu: avalua si és un estat terminal, i si no, genera un fill per cada casella buida, crida recursivament, propaga valors (max o min segons el torn) i poda si `alpha >= beta`.
