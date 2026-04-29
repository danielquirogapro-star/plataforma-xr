# Plataforma XR de Entrenamiento Biomédico

Plataforma AR/VR para entrenar personal médico y técnico en el uso y mantenimiento de equipos biomédicos en hospitales latinoamericanos.

## Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Motor 3D | Unity 2022 LTS |
| XR Framework | Unity XR Toolkit + OpenXR |
| Lenguaje | C# (.NET Standard 2.1) |
| AI Tutor | Claude API (Anthropic) via UnityWebRequest |
| Persistencia local | Unity PlayerPrefs |
| Persistencia nube | Firebase Firestore _(próxima iteración)_ |
| Assets binarios | Git LFS |

## Cómo Correr el Proyecto

### Requisitos
- Unity 2022 LTS (2022.3.x)
- Unity XR Toolkit 2.5+
- Git LFS instalado
- Cuenta Anthropic con API key válida

### Pasos
1. `git clone https://github.com/danielquirogapro-star/plataforma-xr.git`
2. `git lfs pull`
3. Abrir en Unity Hub → Add → seleccionar la carpeta
4. Crear asset `ProjectConfig` en `Assets/_Project/Scripts/Data/` e ingresar la API key
5. Abrir `Assets/_Project/Scenes/MainMenu.unity` → Play

## Módulos del MVP

- [x] Simulador ECG (forma de onda PQRST)
- [x] Simulador SpO2 con alarmas
- [x] Monitor de signos vitales
- [x] Tutor AI con Claude API
- [x] Sistema de progreso y puntaje
- [x] Interacción base con equipos
- [ ] Escenas XR completas _(en desarrollo)_
- [ ] Prefabs 3D del monitor _(en desarrollo)_
- [ ] Integración Firebase _(próxima iteración)_
