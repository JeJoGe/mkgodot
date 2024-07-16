public enum BasicCardActions {
    move, heal, draw, block, discard, ready, influence, attack
}

public enum AttackType {
    ranged, siege, melee
}

public enum AttackBlockElement {
    ice, fire, coldFire, physical
}

public enum SpecialCardActions {
    // next card played gets +# if move, influence, block or any type of attack
    enhanceBottom,
    gainCrystals,
    gainManaTokens,
    useAdditionalDice,
    reduceEnemyAttack,
    reputation,
    fame,
    discardsCards,
    additionalBlock,
    payMana,
    moveSpecial,
    attackSpecial,
    discount
}

public enum CharacterPhases {
    movement,
    interaction,
    combat,
    postEffects
}