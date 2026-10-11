# AGENTS.md - Convenciones del proyecto (SnapClone)

## Contexto

- Unity 2023.1.22f1, juego de cartas 2D por turnos (batalla 1v1, 3 zonas, 6 turnos).
- Arquitectura en dos capas: **Battle Core** (batalla) y **Capa Campaign** (run, draft, recompensas).
- Stack de UI: uGUI + TextMeshPro. Las escenas y prefabs se cablean en el Editor.

## Limitación clave de la IA

La IA **no puede** construir ni modificar de forma fiable lo que requiere el Editor de Unity:

- Crear y asignar **ScriptableObject** assets (`.asset`).
- Arrastrar **referencias serializadas** en el Inspector (campos `public` de los `MonoBehaviour`).
- Editar la **jerarquía de escenas** y la composición de **prefabs**.
- Conectar botones (`onClick`), `Canvas`, `LayoutGroup`, etc.

Por eso, **todo cambio debe entregar además instrucciones escritas** para que un desarrollador cierre el cableado en el Editor. Este archivo es la regla permanente que gobierna esa entrega.

## Regla permanente: Guía de implementación manual

**Regla**: cada mejora que aplique el agente y requiera cableado en el Editor SHALL entregar una **guía Markdown de implementación manual**, en español, junto con el código.

### Cuándo aplica

- Nuevo o modificado **ScriptableObject**: `CardData`, `CardEffectBase`, `PerkData`, `CampaignConfig`, `CharacterData`, `EnemyData`, `DeckData`.
- Nuevo **efecto de carta / habilidad** (`Assets/Scripts/Cards/...`).
- Nuevo o modificado **prefab, escena o UI**.
- Cualquier cambio con **referencias serializadas** o `onClick` a conectar en el Inspector.

### Cuándo NO aplica

- Refactors internos sin cableado (lógica pura, utilidades, tests).
- Cambios que no tocan assets, escenas ni prefabs.

### Ubicación y nombre

```
Código / contenido  ->  <misma carpeta del script nuevo>/<Feature>_GUIDE.md
UI / flujo          ->  Docs/UI/<Feature>_GUIDE.md
```

- Un archivo por mejora. No se sobrescribe una guía de otra feature.
- Unity importará el `.md` como `TextAsset` y generará su `.meta` al abrir el proyecto; no es necesario crearlo a mano.

### Plantilla obligatoria

```markdown
# <Feature> - Guía de implementación manual (Unity Editor)

## 1. Resumen del cambio
Qué se añadió/modificó en código y por qué. Qué NO quedó cableado.

## 2. Archivos de código
- Creados: `ruta/al/NuevoScript.cs`
- Modificados: `ruta/al/Existente.cs`

## 3. Pasos en Unity Editor
1. [ ] Crear el ScriptableObject: `Assets > Create > <menú exacto>`, guardar en `ruta/destino.asset`.
2. [ ] Asignar el asset al campo `<campo>` del asset `<TipoDestino>` en `ruta/destino`.
3. [ ] Registrar en pool / deck / escena: `<dónde>`.
4. [ ] (Si aplica) ajustar valores en el Inspector: `<valores>`.

## 4. Cambios de UI (si aplica)
```
PanelRaiz
+-- Title (TextMeshProUGUI)
+-- Slots
|   +-- Slot1 (Button) -> asignar onClick a <Controller>.Metodo
+-- ...
```
- Componentes a añadir y su GameObject.
- Referencias a arrastrar: `<Controller>.<campo>` <- `<GameObject>`.
- Prefabs reutilizables: `Assets/Prefabs/...`.

## 5. Verificación (checklist en Play)
- [ ] <paso observable 1>
- [ ] <paso observable 2>

## 6. Notas / rollback
Riesgos, dependencias y cómo revertir si algo falla.
```

### Cableado de referencia: efectos de carta (recordatorio)

Los efectos de carta son **ScriptableObjects**, no van en el prefab:

```
[1] Script .cs (subclass CardEffectBase con [CreateAssetMenu])   -> IA
[2] ScriptableObject .asset                                       -> Editor
[3] Asignado a un campo de CardData                               -> Editor
      onRevealEffect / ongoingEffect / conditionalEffect /
      onActivateEffect / onDiscardedEffect / onDestroyEffect / endGameEffect
[4] CardData referenciado por deck / CharacterData / draftPool    -> Editor
[5] CardPrefab.prefab (CardView + CardInstance) instanciado por
      PlayerController.cardPrefab / AIController.cardPrefab       -> runtime
```

Las zonas usan otro patrón: `ZoneEffect` (MonoBehaviour) en `Assets/Prefabs/ZoneEffects/*.prefab`.

## Uso al aplicar un cambio (OpenSpec)

- Antes de cerrar una tarea que crea código/UI, crear o actualizar su `*_GUIDE.md`.
- Marcar en el `tasks.md` del cambio las tareas de guía correspondientes.
- Al archivar, verificar que cada mejora de la fase trae su guía.
