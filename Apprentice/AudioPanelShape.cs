using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Apprentice;

/// <summary>
/// Represents an audio equipment back panel with various connector types.
/// Models a typical professional audio device with XLR, 1/4" jacks, and other connectors.
/// </summary>
public class AudioPanelShape : FoShape3D
{
   public AudioPanelShape(string name, double panelWidth = 16.0, double panelHeight = 4.0) : base(name)
   {
      // Create the main panel body
      CreateBox(name, panelWidth, panelHeight, 1.0);
      Color = "DarkSlateGray";
      
      // Calculate connector spacing
      var spacing = panelWidth / 11.0; // 10 connectors + margins
      var startX = -(panelWidth / 2.0) + spacing;
      
      // Position 01: Combo jack (left side)
      AddConnector("Port_01", "ComboJack", startX + (spacing * 0), 0, 0.6, "cyan");
      
      // Position 02-03: Dual 1/4" jacks
      AddConnector("Port_02", "QuarterInch", startX + (spacing * 1), 0.5, 0.6, "orange");
      AddConnector("Port_03", "QuarterInch", startX + (spacing * 1), -0.5, 0.6, "orange");
      
      // Position 04: XLR connector
      AddConnector("Port_04", "XLR", startX + (spacing * 2.5), 0, 0.6, "silver");
      
      // Position 05: Center ventilation/label area (flat marker)
      AddConnector("Port_05", "Panel", startX + (spacing * 4.5), 0, 0.6, "gray", 0.3);
      
      // Position 06: Connection port
      AddConnector("Port_06", "QuarterInch", startX + (spacing * 6), 0, 0.6, "yellow");
      
      // Position 07: Port
      AddConnector("Port_07", "QuarterInch", startX + (spacing * 7), 0, 0.6, "green");
      
      // Position 08: Additional connection
      AddConnector("Port_08", "QuarterInch", startX + (spacing * 8), 0, 0.6, "blue");
      
      // Position 09: Port
      AddConnector("Port_09", "QuarterInch", startX + (spacing * 9), 0, 0.6, "magenta");
      
      // Position 10: Final connector (right side)
      AddConnector("Port_10", "ComboJack", startX + (spacing * 10), 0, 0.6, "red");
      
      GetTreeNodeTitle().WriteSuccess();
   }
   
   private void AddConnector(string name, string connectorType, double xPos, double yPos, double zPos, string color, double scale = 0.4)
   {
      FoShape3D connector;
      
      switch (connectorType)
      {
         case "XLR":
            // XLR: Cylinder with circular face
            connector = new FoShape3D($"{name}_XLR");
            connector.CreateCylinder($"{name}_XLR", scale * 1.2, scale * 1.2, 0.3);
            connector.Color = color;
            connector.Transform = new Transform3($"{name}_Transform")
            {
               Position = new Vector3(xPos, yPos, zPos),
               Rotation = new Euler(90 * Math.PI / 180, 0, 0) // Rotate to face outward
            };
            AddShape(connector);
            
            // Add center pin indicator
            var pin = new FoShape3D($"{name}_Pin");
            pin.CreateSphere($"{name}_Pin", 0.1, 0.1, 0.1);
            pin.Color = "gold";
            pin.Transform = new Transform3($"{name}_PinTransform")
            {
               Position = new Vector3(xPos, yPos, zPos + 0.2)
            };
            AddShape(pin);
            break;
            
         case "QuarterInch":
            // 1/4" jack: Small cylinder
            connector = new FoShape3D($"{name}_Jack");
            connector.CreateCylinder($"{name}_Jack", scale * 0.6, scale * 0.6, 0.4);
            connector.Color = color;
            connector.Transform = new Transform3($"{name}_Transform")
            {
               Position = new Vector3(xPos, yPos, zPos),
               Rotation = new Euler(90 * Math.PI / 180, 0, 0)
            };
            AddShape(connector);
            break;
            
         case "ComboJack":
            // Combo jack: Larger cylinder with inner ring
            connector = new FoShape3D($"{name}_Combo");
            connector.CreateCylinder($"{name}_Combo", scale * 1.0, scale * 1.0, 0.5);
            connector.Color = color;
            connector.Transform = new Transform3($"{name}_Transform")
            {
               Position = new Vector3(xPos, yPos, zPos),
               Rotation = new Euler(90 * Math.PI / 180, 0, 0)
            };
            AddShape(connector);
            
            // Inner ring
            var ring = new FoShape3D($"{name}_Ring");
            ring.CreateRing($"{name}_Ring", scale * 0.5, scale * 0.5, 0.1);
            ring.Color = "darkgray";
            ring.Transform = new Transform3($"{name}_RingTransform")
            {
               Position = new Vector3(xPos, yPos, zPos + 0.3),
               Rotation = new Euler(90 * Math.PI / 180, 0, 0)
            };
            AddShape(ring);
            break;
            
         case "Panel":
            // Panel marker (ventilation/label area): Flat box
            connector = new FoShape3D($"{name}_Panel");
            connector.CreateBox($"{name}_Panel", scale * 3, scale * 2, 0.1);
            connector.Color = color;
            connector.Transform = new Transform3($"{name}_Transform")
            {
               Position = new Vector3(xPos, yPos, zPos)
            };
            AddShape(connector);
            break;
            
         default:
            // Default: sphere marker
            connector = new FoShape3D($"{name}_Default");
            connector.CreateSphere($"{name}_Default", scale, scale, scale);
            connector.Color = color;
            connector.Transform = new Transform3($"{name}_Transform")
            {
               Position = new Vector3(xPos, yPos, zPos)
            };
            AddShape(connector);
            break;
      }
      
      // Add label text for each connector
      var label = new FoText3D($"{name}_Label")
      {
         Text = name.Replace("Port_", ""),
         FontSize = 0.25,
         Transform = new Transform3($"{name}_LabelTransform")
         {
            Position = new Vector3(xPos, yPos - 0.8, zPos),
         },
         Color = "white"
      };
      AddShape(label);
   }

   public override string GetTreeNodeTitle()
   {
      var pos = Transform!.Position;
      return $"{GetName()} AudioPanel [10 connectors] @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
   }
}
