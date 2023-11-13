/*
 * Shader Id, an easy way to keep id and name connected,
 * also lets you say:
 *     ShaderId foo = "lala"
 * rather than
 *     int foo = Shader.PropertyToID("Lala");
 */

using UnityEngine;

namespace Blep.TextureOps {

public struct ShaderId  {

    public readonly string name;
    public readonly int id;

    public ShaderId(string name) {
        this.name = name;
        this.id = Shader.PropertyToID(name);
    }

    public override string ToString() => name;

    // Implciitly creates ShaderId from string. Let's you write:
    //    ShaderId shadowColorId = "_ShadowColor";
    static public implicit operator ShaderId(string name) => new ShaderId(name);

    // Implicitly converts ShaderId to int. Let's you write:
    //    material.SetColor(shadowColorId, Color.white);
    static public implicit operator int(ShaderId sid) => sid.id;
}
}
