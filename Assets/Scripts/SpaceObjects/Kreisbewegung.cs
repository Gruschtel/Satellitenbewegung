using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
Create by Eric G. Werner
 */
public class Kreisbewegung : MonoBehaviour
{
    // ============================================================
    // Fields
    // ============================================================
    [SerializeField] private Toggle SatellitenToggle = null; // Statische Umlaufbahnen darstellen
    [SerializeField] private Toggle UmlaufbahnToggle = null; // Dynamische Umlaufbahnen darstellen
    [SerializeField] private Slider WorldSpeedSlider = null; // Geschwindigkeitsregler
    [SerializeField] private GameObject centerObject = null; // Objekt um das rotiert werden soll
    [SerializeField] private bool clockwiseDirection = false; // Mit dem Uhrzeiger
    [SerializeField] private float startAngle = 0.0f; // Wo ist das Objekt zu Beginn? 
    [SerializeField] private float startBias = 0.0f; // Wo ist das Objekt zu Beginn? 
    [SerializeField] private float startProgress = 0.0f; // Wo ist das Objekt zu Beginn? 
    [SerializeField] private float orbitSpeed = 1.0f; // Wie schnell ist die Drehung?
    [SerializeField] private float orbitTrans = 1.0f; // Entfernung zum CenterObjekt
    [SerializeField] private LineRenderer lineRenderer = null; // LineRender zum erzeugen von linen
    [SerializeField] private int rendererAccuracy = 20; // Genauigkeit des LineRenderer

    private List<Vector3> LineRendererPositions = new List<Vector3>(); // Speicherung zwischen Positionen für den LineRenderer
    private bool resetLineList = false; // Zwischenspeicherung ob im nächsten Update der LineRendere zurück gesetzt werden soll (Ein Toggle wurde bestätigt)
    private bool showStaticLines = false; // Zwischenspeicherung ob Static Umlaufbahnen dargestellt werden sollen oder nicht
    private bool ShowDynamicLine = false; // Zwischenspeicherung ob Dynamic Umlaufbahnen dargestellt werden sollen oder nicht
    private int progressLineRenderer = 0; // Aktueller Fortschritt des LineRenderer
    private int currentLineRendererPostion = 0; // Aktuelle Position des LineRenderer
    private float world_speed = 1; // Weltgeschwindigkeit
    private Vector3 direction; // Richtungsvektor
    private GameObject moveObject = null; // Unsichtbares Objekt zum berechnen der Umlaufbahnen

    // Start is called before the first frame update
    void Start()
    {
        // StartAngle eingrenzen -90 -- 0 -- 90
        if (!Mathf.Approximately(Mathf.Abs(startAngle), 90f))
        {
            float tempAngle = Mathf.Abs(startAngle) % 90f;
            if (startAngle < 0f) startAngle = tempAngle * -1f;
            else startAngle = tempAngle;
        }

        // StartBias eingrenzen -180 -- 0 -- 180
        if (!Mathf.Approximately(Mathf.Abs(startBias), 180f))
        {
            float tempBias = Mathf.Abs(startBias) % 180f;
            if (startBias < 0f) startBias = tempBias * -1f;
            else startBias = tempBias;
        }

        // WorldSpeedSlider zuweisen
        if (WorldSpeedSlider != null)
        {
            WorldSpeedSlider.onValueChanged.AddListener(delegate { ChangeWorldSpeed(); });
            world_speed = WorldSpeedSlider.value;
        }

        // Statische Umlaufbahn zuweisen
        if (SatellitenToggle != null)
        {
            SatellitenToggle.onValueChanged.AddListener(delegate { ChangeStaticLine(); });
            showStaticLines = SatellitenToggle.isOn;
        }

        // Dynamische Umlaufbahn zuweisen
        if (UmlaufbahnToggle != null)
        {
            UmlaufbahnToggle.onValueChanged.AddListener(delegate { ChangeDynamicLine(); });
            ShowDynamicLine = UmlaufbahnToggle.isOn;
        }

        // Mindest LineRender Genauigkeit festlegen
        if (rendererAccuracy <= 10)
            rendererAccuracy = 10;

        // setzt Startposition des SpaceObjects neben dem SpaceObject um den es kreisen soll
        // Wenn == null, dann verschiebe das GameObjekt nicht
        // Wenn != null, dann verschiebe GameObject zu CenterObject
        if (centerObject == null)
        {
            Debug.LogWarning(name + ": No Center Object");
        }
        else
        {
            // GameObject wird auf die Oberfläche des CenterObject verschoben
            Vector3 temp = new Vector3(centerObject.transform.position[0],
                centerObject.transform.position[1],
                centerObject.transform.position[2] + (centerObject.transform.localScale[2]) / 2f + orbitTrans);

            gameObject.transform.position = temp;

            // GameObject wird entsprechend seiner StartAngle und StartBias rotiert
            transform.RotateAround(centerObject.transform.position, Vector3.left, startAngle);
            transform.RotateAround(centerObject.transform.position, Vector3.down, startBias);

            // GameObject wird auf seiner Umlaufbahn um das CenterObject verschoben
            if (!Mathf.Approximately(startProgress, 0.0f))
            {
                transform.RotateAround(centerObject.transform.position, getDirection(), startProgress);
            }

            // Die Richtung des GameObjects wird ermittelt
            direction = getDirection();

        }

        // Wenn LineRenderer vorhanden, erzeuge ein Object zum späteren zeichnen der Linien
        if (lineRenderer != null)
            moveObject = Instantiate(new GameObject(), transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // Wenn == null, dann soll sich das GameObejct um sich selbst drehen
        // Wenn != null, dann rotiere das GameObject um das CenterObject
        if (centerObject == null)
        {
            if (clockwiseDirection)
            {
                transform.Rotate(Vector3.up, orbitSpeed * Time.deltaTime * world_speed);
            }
            else
            {
                transform.Rotate(-Vector3.up, orbitSpeed * Time.deltaTime * world_speed);
            }
        }
        else
        {
            direction = getDirection();
            
            // rotiert das GameObject um das CenterObject 
            transform.RotateAround(centerObject.transform.position, direction, orbitSpeed * Time.deltaTime * world_speed);
            
            // Zeichnet die Linien der Umlaufbahnen
            // Satelliten können zwischen Dynamischen und Statischen Umlaufbahnen wechseln
            // Außerdem soll nur Satelliten sich zum CenterObject ausrichten, andere Objecte nicht
            if (tag.Equals("Satellit"))
            {
                transform.LookAt(centerObject.transform.position);
                if (ShowDynamicLine || showStaticLines)
                    if (showStaticLines)
                    {
                        
                        // Statische Umlaufbahn
                        DrawPath();
                    }
                    else if (ShowDynamicLine)
                    {
                        
                        // Dynamische Umlaufbahn
                        // Akutell wird die zukünftige Flugbahn dargestellt
                        // DrawDynamicTail = Stellt die Gefolgene Flugbahn da --> Ähnlich einem Sternenschweif
                        DrawDynamicPath();
                        // DrawDynamicTail();
                    }
            }
            
            // Planenten können nur ihre Statische Umlaufbahn darstellen, gleichgültig ob Statisch oder Dynamisch 
            else if (tag.Equals("Planet"))
            {
                if (ShowDynamicLine || showStaticLines)
                {
                    DrawPath();
                }
            }
        }
    }

    // Bestimmt die richtungsvektoren für das rotate
    public Vector3 getDirection()
    {
        Vector3 direction = new Vector3(0, 0, 0);
        // ==================================
        // StartAngle: 0 Grad
        // ==================================
        if (Mathf.Approximately(startAngle, 0.0f))
        {
            direction = new Vector3(0.0f, -1.0f, 0.0f);
        }
        // ==================================
        // StartAngle: 90 oder - 90 Grad
        // ==================================
        else if (Mathf.Approximately(Mathf.Abs(startAngle), 90f))
        {
            //
            // StartBias:
            //

            // 0 Grad
            if (Mathf.Approximately(startBias, 0.0f))
                direction = new Vector3(-1.0f, 0.0f, 0.0f);
            
            // - 90 oder 90
            else if (Mathf.Approximately(Mathf.Abs(startBias), 90f))
                direction = new Vector3(0.0f, 0.0f, -1.0f);
            
            // Diagonale 1 bis 89 und -91 bis -179
            else if ((startBias > 0f && startBias < 90f) || (startBias > -90f && startBias < -180f))
            {
                direction = new Vector3(1.0f, 0.0f, -1.0f);
            }
            
            // Diagonale -1 bis -89 und 91 bis 179
            else
            {
                direction = new Vector3(1.0f, 0.0f, 1.0f);
            }
        }
        // ==================================
        // Diagonale 
        // ==================================
        else
        {
            //
            // Start Bias:
            //
            
            // 44 -- 0 -- -44 
            if ((startBias >= 0f && startBias < 45f) || (startBias < 0 && startBias >= -45f))
            
                direction = new Vector3(1.0f, -1.0f, 1.0f);
            
            // 45 -- 90 -- 134
            else if (startBias < 135f && startBias >= 45f)
                direction = new Vector3(1.0f, 1.0f, -1.0f);
            
            // -45 -- -90 -- -134
            else if (startBias < -45f && startBias >= -135f)
                direction = new Vector3(1.0f, -1.0f, -1.0f);
            
            // 135 -- 180
            else if ((startBias > 135f && startBias <= 180) || (startBias < -135f && startBias >= -180f))
                direction = new Vector3(-1.0f, -1.0f, -1.0f);
            
            // -135 -- -180
            if (startAngle < 0)
                direction = new Vector3(direction[0] * 1f, direction[1] * -1f, direction[2] * 1f);
        }

        // Wenn ClockwiseDirection, muss der Richtungsvektor in die Gegengensgesetzt richtung gebracht werden
        if (clockwiseDirection)
        {
            direction = new Vector3(direction[0] * -1f, direction[1] * -1f, direction[2] * -1f);
        }
        return direction;
    }

    // Ermöglicht es die geflogene Flugbahn eines Satellit darzustellen
    private void DrawDynamicTail()
    {
        if (lineRenderer == null)
            return;
        // Delete old Positions, when activate dynamic lines
        if (resetLineList && LineRendererPositions.Count > 0)
        {
            // testListe = new List<Vector3>(); or testListe.RemoveAll didn't work
            for (int i = 0; i <= LineRendererPositions.Count; i++)
            {
                LineRendererPositions.RemoveAt(0);
            }
            lineRenderer.positionCount = currentLineRendererPostion = 0;
        }
        resetLineList = false;

        // dont save and show start position --> (0,0,0)
        if (progressLineRenderer == 1)
        {
            lineRenderer.positionCount = currentLineRendererPostion = 0;
        }

        int tempAccuracy = 10;
        
        // Da Update ziemlich oft aufgerufen wird und damit die Funktion --> Führt zu Lags und Darstellungsfehler
        // Linie wird nur alle x Schritte gezeichnet
        if (progressLineRenderer % (360 / rendererAccuracy) == 0)
        {
        
            // Begrenzt länge der Linie
            // < tempAccurarcy = Erzeugt Linie
            // > tempAccuracy = Verschiebt Linie mit dem Satelliten
            if (currentLineRendererPostion < tempAccuracy)
            {
                lineRenderer.positionCount = currentLineRendererPostion + 1;
                lineRenderer.SetPosition(currentLineRendererPostion++, new Vector3(transform.position.x, transform.position.y, transform.position.z));
                LineRendererPositions.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
            else
            {
                // Löscht erstes Element aus der Liste
                LineRendererPositions.RemoveAt(0);
                lineRenderer.positionCount = tempAccuracy + 1;

                // Erzeugt ein neues Element und fügt es am Ende der Liste ein --> Verschiebungseffekt
                lineRenderer.SetPositions(LineRendererPositions.ToArray());
                lineRenderer.SetPosition(tempAccuracy, new Vector3(transform.position.x, transform.position.y, transform.position.z));
                LineRendererPositions.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
        }
        // Fortschritt wird gespeichert
        progressLineRenderer++;
    }

    // Möglichkeit die Dynamische Flugbahn der Satelliten zu erzeugen
    private void DrawDynamicPath()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = (rendererAccuracy / 10);
        float tempAccuracy = 360f / rendererAccuracy;
        moveObject.transform.position = gameObject.transform.position;
        for (int i = 0; i < rendererAccuracy / 10; i++)
        {
            // erzeugt linien
            lineRenderer.SetPosition(i, new Vector3(moveObject.transform.position.x, moveObject.transform.position.y, moveObject.transform.position.z));
            // Verschiebt das unsichtbare Object ein Schritt nach vorne und speichert position (nächste Linie wird zu der letzten position dieses Objects gezeichnet)
            moveObject.transform.RotateAround(centerObject.transform.position, direction, ((orbitSpeed * world_speed) / 5) + 1f);
        }


    }

    // Erzeugt Statische Umlaufbahnen der Objecte
    private void DrawPath()
    {
        if (lineRenderer == null)
            return;

        moveObject.transform.position = gameObject.transform.position;
        lineRenderer.positionCount = rendererAccuracy + 1;
        float tempAccuracy = 360f / rendererAccuracy;
        for (int i = 0; i <= rendererAccuracy; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(moveObject.transform.position.x, moveObject.transform.position.y, moveObject.transform.position.z));
            moveObject.transform.RotateAround(centerObject.transform.position, direction, tempAccuracy);
        }
    }

    // Registriert Änderungen am World Speed
    public void ChangeWorldSpeed()
    {
        world_speed = WorldSpeedSlider.value;
    }

    // Registriert Änderungungen am StaticToggle
    public void ChangeStaticLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = SatellitenToggle.isOn;
            showStaticLines = SatellitenToggle.isOn;
            if (showStaticLines)
            {
                if (UmlaufbahnToggle != null) UmlaufbahnToggle.isOn = false;
                ShowDynamicLine = false;
                lineRenderer.positionCount = currentLineRendererPostion = 0;
                // DrawPath();
            }
        }
    }

    // Registiert Änderungen am DynamicToggle
    public void ChangeDynamicLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = UmlaufbahnToggle.isOn;
            ShowDynamicLine = UmlaufbahnToggle.isOn;
            if (ShowDynamicLine)
            {
                SatellitenToggle.isOn = false;
                showStaticLines = false;
                resetLineList = true;
                lineRenderer.positionCount = currentLineRendererPostion = 0;
            }
        }
    }
}
