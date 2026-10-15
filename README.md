EscapeRoom3D

Equipo de desarrollo: 4 integrantes
Integrantes
Julia Rafaela Herrera Heiden
Tanina Magdiel Soliz Quizpe
Nicole Patino Guzman 
OTONIEL ALAIN PEREZ LEDEZMA

##Modificación para caso 5

Proyecto: Escape Room 3D con Unity.

## Notas del jugador
- PlayerController escribe "Movement system active" una sola vez al iniciar (Start) para no llenar la Console en cada frame.
- Tecla E: interacción.

## El Estudio del Coleccionista (diseño del cuarto)
Un estudio victoriano con muebles de Poly Haven y Sketchfab (créditos en `Assets/04_Models/CREDITOS.txt`).
- **Construirlo:** abrir `MainScene` y usar el menú **Escape Room > Construir el Estudio del Coleccionista**.
  Necesita el paquete glTFast (ya está en `Packages/manifest.json`). Se puede volver a ejecutar cuando se quiera.
- **Controles:** WASD moverse, ratón o Q/R girar, E interactuar, Shift correr. Clic para capturar el ratón.
- **Acertijo:** la nota sobre la consola dice el orden (tiempo, arte, mundo, saber). El reloj marca la 1,
  el cuadro tiene un 2, el globo tiene 3 alfileres y del librero sobresalen 4 libros → código **1234**
  (el mismo `correctCode` de PuzzleManager). El candado abre el cofre, el cofre da la llave y la llave abre la puerta.
- El panel de prueba (`PuzzleTest`) queda apagado; se vuelve a usar activando su componente en el Inspector.
