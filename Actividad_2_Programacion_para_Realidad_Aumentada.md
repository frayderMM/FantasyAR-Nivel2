# SEGUNDA ACTIVIDAD

# PROGRAMACIÓN PARA REALIDAD AUMENTADA

**Curso:** Programación para Realidad Aumentada  
**Periodo:** 2026-2

## Instrucciones

- Redacte todas sus respuestas en el cuadernillo de Word.
- Escriba con lapicero si lo hace a mano.
- Trabaje con orden y limpieza.
- Solo debe subir un integrante por grupo.
- Colocar el nombre y el apellido de cada integrante.
- **Fecha:** 13/11/2026

## Caso: Sistema de Realidad Aumentada para el Festival Medieval FantasyAR

El Festival Medieval FantasyAR es un evento de entretenimiento temático que busca transportar a sus visitantes a un universo de fantasía medieval, donde conviven caballeros, hechiceros, criaturas míticas y personajes legendarios. El festival reúne a miles de aficionados a los juegos de rol, la literatura fantástica, los videojuegos y las historias de aventuras, y ofrece exhibiciones de armaduras, armas legendarias, escenarios medievales y representaciones de personajes y criaturas fantásticas. Asimismo, se organizan concursos de cosplay, espectáculos de magia, demostraciones de combate medieval y diversas actividades interactivas dirigidas a públicos de diferentes edades.

Con el objetivo de modernizar las exhibiciones y ofrecer experiencias más inmersivas, los organizadores desean incorporar tecnologías de Realidad Aumentada que permitan a los visitantes observar e interactuar con personajes y criaturas del universo medieval fantástico. A través de dispositivos móviles, los asistentes podrán visualizar modelos tridimensionales integrados en el entorno real, apreciar sus movimientos, observar distintas animaciones y experimentar efectos visuales relacionados con sus características y habilidades.

Para implementar esta propuesta, una empresa especializada en experiencias digitales ha sido contratada para desarrollar una aplicación llamada FantasyAR, orientada a la visualización de personajes y criaturas fantásticas mediante realidad aumentada. La aplicación permitirá reconocer diferentes elementos visuales y activar modelos 3D animados, acompañados de efectos visuales que simulen habilidades mágicas, poderes especiales o características propias de cada personaje.

Antes de implementar la aplicación durante el festival, los organizadores han solicitado desarrollar y evaluar un prototipo funcional de Realidad Aumentada de Nivel 2. El prototipo deberá cumplir con las siguientes especificaciones:

## Nivel 2

### 1. MultiTarget

Utilizar un MultiTarget con estructura Cuboid, empleando Merge Cuboid, que permita visualizar un personaje o criatura de fantasía medieval mediante Realidad Aumentada a través del reconocimiento de la estructura.

La elección del personaje o de la criatura queda a criterio del estudiante. Se podrán utilizar modelos 3D de caballeros, magos, elfos, orcos, vampiros, hombres lobo, dragones, hechiceras, gólems, trolls, grifos u otros personajes y criaturas del universo de la fantasía medieval.

El modelo 3D deberá cubrir toda la estructura del Cuboid, con una escala, posición y orientación adecuadas para su correcta visualización.

### 2. Dibujos digitales como Image Targets

Crear tres (3) dibujos digitales originales que servirán como Image Targets para visualizar tres modelos 3D distintos relacionados con la temática de la fantasía medieval.

Cada dibujo digital deberá representar visualmente al personaje o criatura seleccionada y estar directamente relacionado con su respectivo modelo 3D.

Los dibujos digitales deberán cumplir las siguientes características:

- Cada dibujo deberá estar relacionado con el personaje o la criatura fantástica seleccionada.
- Se permitirá utilizar plataformas de dibujo digital, herramientas de Inteligencia Artificial (IA) o aplicaciones similares para su elaboración.

### 3. Modelos 3D y efectos visuales

Se deberán utilizar cuatro modelos 3D diferentes: uno correspondiente al MultiTarget y tres asociados a los dibujos digitales.

Los modelos 3D deberán obtenerse de las siguientes fuentes:

- Dos (2) modelos 3D deberán obtenerse de Mixamo.
- Dos (2) modelos 3D podrán obtenerse de fuentes externas a Mixamo, como la Unity Asset Store u otras bibliotecas de modelos 3D revisadas durante el curso.

Cada uno de los cuatro modelos 3D deberá incorporar como mínimo:

- Un (1) efecto visual (VFX) relacionado con el personaje o la criatura, como fuego, hielo, electricidad, humo, magia, aura, chispas, energía o niebla.
- Los efectos visuales podrán obtenerse en la Unity Asset Store.

### 4. Animaciones

Cada uno de los cuatro modelos 3D deberá incorporar obligatoriamente tres (3) animaciones diferentes, obtenidas de Mixamo:

- **Spell (Hechizo):** animación en la que el personaje lanza un hechizo o realiza una acción mágica.
- **Idle (Reposo):** animación en la que el personaje permanece inmóvil y realiza movimientos naturales.
- **Jump (Saltar):** animación en la que el personaje salta.

Las animaciones deberán cumplir las siguientes condiciones:

- Las tres animaciones deberán obtenerse de Mixamo e integrarse correctamente en Unity mediante un Animator Controller.
- No se deberán repetir las animaciones entre los cuatro modelos 3D. Cada modelo deberá utilizar versiones distintas de Spell, Idle y Jump, evitando emplear exactamente los mismos movimientos en diferentes personajes o criaturas.
- Las animaciones deberán ejecutarse correctamente y corresponder a los movimientos establecidos.
- Se deberá garantizar una transición adecuada entre las animaciones del mismo modelo 3D.

### 5. Botones virtuales

En uno de los tres dibujos digitales utilizados como Image Targets, se deberán implementar tres (3) botones virtuales que permitan controlar individualmente las animaciones del modelo 3D correspondiente.

Cada botón virtual deberá ejecutar una animación específica:

- **Botón 1 – Spell (Hechizo):** activa la animación de hechizo.
- **Botón 2 – Idle (Reposo):** activa la animación de reposo.
- **Botón 3 – Jump (Saltar):** activa la animación de salto.

Los botones virtuales deberán cumplir las siguientes características:

- Estar asociados al Image Target seleccionado y ubicados en posiciones que permitan su correcta identificación y activación.
- El funcionamiento de cada botón virtual deberá implementarse mediante un script en C# que active la animación correspondiente del modelo 3D.
- Los scripts podrán desarrollarse con el apoyo de plataformas de Inteligencia Artificial (IA), siempre que se integren correctamente en Unity y permitan ejecutar las funciones establecidas.
- Activar únicamente la animación del botón seleccionado.
- Permitir cambiar entre las tres animaciones mediante la interacción del usuario.

## Consideraciones del Nivel 2

- Los tres dibujos digitales deberán elaborarse de manera original utilizando herramientas de dibujo, de diseño gráfico o plataformas de inteligencia artificial.
- Cada dibujo digital deberá estar directamente relacionado con el personaje o la criatura representado por su modelo 3D.
- Los dibujos deberán presentar suficientes características visuales distintivas, detalles distribuidos y contrastes adecuados para facilitar su reconocimiento mediante Vuforia.
- Los tres modelos 3D asociados a los dibujos digitales deberán ser distintos entre sí y no repetirse con el modelo utilizado en el MultiTarget.
- Dos modelos 3D deberán obtenerse de Mixamo y los otros dos podrán provenir de fuentes externas, siempre que respeten la temática establecida.
- Todos los modelos 3D deberán incluir sus texturas correspondientes y configurarse con una escala, posición y orientación adecuadas.
- Cada modelo 3D deberá incorporar las tres animaciones obligatorias: Spell (Hechizo), Idle (Reposo) y Jump (Saltar), obtenidas de Mixamo e integradas mediante un Animator Controller. No se permitirá repetir exactamente las mismas animaciones entre los diferentes modelos 3D.
- Los efectos visuales (VFX) podrán descargarse de la Unity Asset Store.
- Los tres botones virtuales deberán implementarse en uno de los dibujos digitales, permitiendo controlar individualmente las tres animaciones del modelo 3D asociado mediante scripts en C#.
- Se permitirá utilizar plataformas de Inteligencia Artificial para generar los scripts de los botones virtuales, siempre que estos funcionen correctamente en Unity y Vuforia.
- Se deberá comprobar que los cuatro modelos 3D presenten correctamente sus respectivas animaciones y efectos visuales, así como que funcionen los botones virtuales en el Image Target seleccionado.
- Tanto el MultiTarget (Cuboid mediante Merge Cuboid) como los tres dibujos digitales deberán adjuntarse como evidencia del desarrollo de la actividad.

## Se pide

### 1. Grabación de funcionamiento

Realizar una grabación de las pruebas de funcionamiento de la Realidad Aumentada de Nivel 2, mostrando:

- El reconocimiento del MultiTarget (Cuboid mediante Merge Cuboid) y de los tres dibujos digitales.
- La visualización de los cuatro modelos 3D con sus respectivas texturas.
- El funcionamiento de las tres animaciones Spell, Idle y Jump de cada modelo, sin repetir los mismos movimientos entre los modelos.
- La activación del efecto visual (VFX) correspondiente a cada modelo.
- El funcionamiento de los tres botones virtuales mediante sus respectivos scripts en C#.

La grabación deberá contar con buena iluminación, enfoque y estabilidad. Se deberán considerar al menos 10 segundos para el reconocimiento y la visualización de cada marcador, además del tiempo necesario para mostrar sus animaciones, efectos visuales y botones virtuales.

**El incumplimiento de este requisito conlleva automáticamente una calificación de 0 puntos.**

### 2. Archivos utilizados

Adjuntar los archivos correspondientes a:

- Tres dibujos digitales utilizados como Image Targets.
- Cuatro modelos 3D con sus texturas.
- Animaciones y efectos visuales (VFX).
- Scripts en C# para los botones virtuales.
- Video de demostración.

Todos los archivos deberán organizarse en carpetas claramente identificadas.

**El incumplimiento de este requisito conlleva automáticamente una calificación de 0 puntos.**

### 3. Enlace de Google Drive

Subir los archivos y la grabación a una carpeta de Google Drive, con permisos de visualización habilitados, acceso sin necesidad de autorización adicional y disponibilidad durante todo el proceso de evaluación.

**El incumplimiento de este requisito conlleva automáticamente una calificación de 0 puntos.**
