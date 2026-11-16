# 🏰 FantasyAR – Realidad Aumentada Nivel 2

Prototipo funcional de **Realidad Aumentada** para el **Festival Medieval FantasyAR**, un evento temático de fantasía medieval. La aplicación reconoce marcadores físicos con un dispositivo móvil y muestra **personajes 3D animados** acompañados de **efectos visuales** de magia, fuego y energía, integrados en el entorno real.

> Segunda actividad del curso **Programación para Realidad Aumentada (2026-2)**.

---

## ✨ Funcionalidades

| Marcador | Tipo | Personaje | Fuente del modelo | Efecto visual (VFX) |
|---|---|---|---|---|
| Merge Cube (6 cm) | **MultiTarget Cuboid** | Caballero | Mixamo | ⚡ Electricidad en ambas manos |
| Dibujo 1 | Image Target + **3 botones virtuales** | Mago | Mixamo | ✨ Aura rúnica + rayo al terminar el hechizo |
| Dibujo 2 | Image Target | Orco | Tripo 3D (rig de Mixamo) | 🔥 Fuego en ambas manos |
| Dibujo 3 | Image Target | Elfo | Tripo 3D (rig de Mixamo) | 🌟 Brillo en la espada + estrellas mágicas |

- **Animaciones:** cada personaje tiene **Spell (Hechizo)**, **Idle (Reposo)** y **Jump (Salto)**: 12 animaciones distintas de Mixamo, sin repetir entre personajes, integradas con **Animator Controllers** y transiciones suaves.
- **Ciclo automático:** el Caballero, el Orco y el Elfo recorren Idle → Spell → Idle → Jump por sí solos.
- **Botones virtuales:** en el dibujo del Mago hay 3 sellos que funcionan como botones: 🔥 **Spell**, 🌙 **Idle** y ⬆️ **Jump**. Al taparlos con el dedo se activa **únicamente** la animación de ese botón.
- **Orientación inteligente:** los personajes se mantienen de pie y de frente a la cámara, tanto si el dibujo está sobre una mesa como si se mira de frente.
- **Dibujos originales:** creados con IA y optimizados para Vuforia (muchos detalles y alto contraste).

### Animaciones utilizadas (Mixamo)

| Personaje | Spell | Idle | Jump |
|---|---|---|---|
| Caballero | Standing 1H Cast Spell 01 | Idle | Jumping |
| Mago | Standing 2H Cast Spell 01 | Idle | Standing Jump |
| Orco | Standing 2H Magic Area Attack 02 | Orc Idle | Jump Attack |
| Elfo | Sword And Shield Casting | Happy Idle | Jump |

---

## 🔘 Botones virtuales en Vuforia 11

Vuforia retiró los *Virtual Buttons* en la versión 11 (estaban obsoletos desde la 10.17). Por eso se implementó un **sistema propio en C#** que funciona igual:

1. Obtiene la imagen de la cámara en escala de grises (`CameraDevice.GetCameraImage`).
2. Proyecta una rejilla de puntos de cada sello sobre esa imagen, usando la pose del Image Target y los parámetros de la lente (`GetCameraIntrinsics`).
3. Compara la zona con el dibujo original mediante **correlación cruzada normalizada**, que no se ve afectada por cambios de luz.
4. Si un sello deja de parecerse al dibujo (un dedo lo tapa) mientras los demás siguen visibles, el botón se considera **presionado** y se activa su animación.

---

## 🛠️ Tecnologías

- **Unity 6.5** (6000.5.9f1) con **Universal Render Pipeline (URP)**
- **Vuforia Engine 11.4.4**
- **C#**
- **Mixamo**: modelos y animaciones
- **Tripo 3D**: modelos del Orco y del Elfo
- **Cartoon FX Remaster Free** (Jean Moreno, Unity Asset Store): efectos visuales
- Plataforma: **Android 10 o superior** (ARM64, IL2CPP)

## 📜 Scripts principales

| Script | Función |
|---|---|
| `BotonVirtualAR.cs` | Define cada botón virtual: zona del dibujo y eventos |
| `DetectorBotonesVirtuales.cs` | Detecta qué sello está tapado leyendo la imagen de la cámara |
| `ControlBotonesMago.cs` | Activa Spell, Idle o Jump del Mago y lanza el rayo |
| `OrientarHaciaCamara.cs` | Mantiene a los personajes de pie y de frente a la cámara |
| `ConfigurarCamaraAR.cs` | Modo de cámara en calidad y enfoque automático continuo |
| `Editor/ConfiguradorFantasyAR.cs` | Arma automáticamente la escena, los Animators y los efectos (menú **FantasyAR**) |

## 📁 Estructura

```
Assets/FantasyAR/
├── Animaciones/   12 animaciones de Mixamo (por personaje)
├── Animators/     4 Animator Controllers
├── Editor/        Configurador automático de la escena
├── Modelos/       4 modelos 3D con sus texturas y materiales
├── Scenes/        FantasyAR_Nivel2.unity
├── Scripts/       Scripts C# (botones virtuales, orientación, cámara)
└── Texturas/      Dibujo de referencia para los botones virtuales
Assets/StreamingAssets/Vuforia/   Base de datos de Vuforia (Cuboid + 3 dibujos)
Recursos/
├── 01_Dibujos/ImageTargets/   Los 3 dibujos originales
└── 05_Scripts/                Copia de los scripts C#
```

## ▶️ Cómo ejecutarlo

1. Descargar **Vuforia Engine 11.4.4** desde [developer.vuforia.com](https://developer.vuforia.com/downloads/sdk) y colocar el archivo `com.ptc.vuforia.engine-11.4.4.tgz` en la carpeta `Packages/` (no se incluye por su tamaño: 132 MB).
2. **Antes de abrir Unity**, copiar los modelos y animaciones FBX (no se incluyen por tamaño y licencia de Mixamo/Tripo 3D) con estos nombres exactos, para que la escena los reconozca:
   - `Assets/FantasyAR/Modelos/<Personaje>/<Personaje>.fbx` (Caballero, Mago, Orco, Elfo), con sus texturas en la subcarpeta `Textures/`.
   - `Assets/FantasyAR/Animaciones/<Personaje>/<Personaje>_<Spell|Idle|Jump>_<NombreMixamo>.fbx` (ver la tabla de animaciones).
3. Abrir el proyecto con **Unity 6000.5.9f1**.
4. Importar **Cartoon FX Remaster Free** desde la Unity Asset Store (gratuito; no se incluye en el repositorio por su licencia).
5. Colocar tu propia **License Key de Vuforia** en *Window → Vuforia Configuration* (no se incluye).
6. Abrir la escena `Assets/FantasyAR/Scenes/FantasyAR_Nivel2.unity`.
7. Imprimir los dibujos a **19 cm de ancho** y armar un **Merge Cube de 6 cm**.
8. Darle **Play** (con webcam) o compilar para **Android**.

---

## 📌 Créditos

- Modelos y animaciones: [Mixamo](https://www.mixamo.com) (Adobe) y Tripo 3D.
- Efectos visuales: *Cartoon FX Remaster Free*, de Jean Moreno (Unity Asset Store).
- Reconocimiento de imágenes: [Vuforia Engine](https://developer.vuforia.com).
- Merge Cube: Merge Labs.
