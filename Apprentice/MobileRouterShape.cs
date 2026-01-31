using FoundryWorldsAndDrawings.Shape;
using FoundryMicroCore.Core.Extensions;
using FoundryMicroCore.Core;
using FoundryWorldsAndDrawings;
using FoundryMicroCore.Core;
using FoundryRulesAndUnits.Extensions;
using FoundryMicroCore.Core;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMicroCore.Core;

namespace Three2025.Apprentice;

/// <summary>
/// Represents a 4G/LTE mobile broadband router with cellular WAN and wired Ethernet LAN.
/// Models connectors, LEDs, and physical layout of a typical mobile router device.
/// </summary>
public class MobileRouterShape : FoShape3D
{
   /// <summary>
   /// Default formatter for mobile router - shows name, feature count, and position
   /// </summary>
   public static new readonly Func<MxObject, string> DefaultFormatter = g => 
   {
      if (g is FoShape3D shape && shape.Transform?.Position != null)
      {
         var pos = shape.Transform.Position;
         return $"{g.Name} MobileRouter [1 PWR, 1 SIM, 2 RJ45, 8 LEDs] @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
      }
      return $"{g.Name} MobileRouter";
   };

   public MobileRouterShape(string name, double width = 16.0, double height = 3.0, double depth = 1.5) : base(name)
   {
      // Set the formatter to use our static equipment formatter
      MxObject.SetCustomTreeViewNodeTitleFunction(this, DefaultFormatter);
      
      // Create the main router chassis (wider, flatter)
      CreateBox(name, width, height, depth);
      Color = "DimGray";
      
      var halfWidth = width / 2.0;
      var halfHeight = height / 2.0;
      var halfDepth = depth / 2.0;
      
      // LEFT SECTION: Power Socket and LED
      AddPowerSocket("PowerSocket", -halfWidth + 1.5, 0, -halfDepth + 0.3);
      AddLED("PWR_LED", -halfWidth + 1.5, -halfHeight + 0.8, halfDepth + 0.05, "lime", "PWR");
      
      // CENTER SECTION: Mobile Network Components
      
      // SIM Card Holder (center bottom)
      AddSIMHolder("SIMHolder", 0, -halfHeight + 0.3, -halfDepth + 0.3);
      
      // Mobile Network Type LEDs (3G, 4G) - upper center left
      AddLED("3G_LED", -3.0, halfHeight - 0.5, halfDepth + 0.05, "blue", "3G");
      AddLED("4G_LED", -1.5, halfHeight - 0.5, halfDepth + 0.05, "blue", "4G");
      
      // Signal Strength LEDs (3 bars) - upper center, tighter spacing
      AddSignalBar("Signal_Bar_1", 0.5, halfHeight - 0.5, halfDepth + 0.05, 0.3, "yellow");
      AddSignalBar("Signal_Bar_2", 1.2, halfHeight - 0.5, halfDepth + 0.05, 0.5, "yellow");
      AddSignalBar("Signal_Bar_3", 1.9, halfHeight - 0.5, halfDepth + 0.05, 0.7, "yellow");
      
      // RIGHT SECTION: RJ45 Ethernet Ports (side by side)
      
      // LAN Port
      AddRJ45Port("LAN_Port", halfWidth - 2.0, 0.6, 0, "LAN");
      AddLED("LAN_LED", halfWidth - 2.0, -halfHeight + 0.8, halfDepth + 0.05, "green", "LAN");
      
      // WAN Port
      AddRJ45Port("WAN_Port", halfWidth - 2.0, -0.6, 0, "WAN");
      AddLED("WAN_LED", halfWidth - 0.8, -halfHeight + 0.8, halfDepth + 0.05, "green", "WAN");
      
      // Mounting screw holes (corners)
      AddScrewHole("Screw_TL", -halfWidth + 0.5, halfHeight - 0.5, halfDepth);
      AddScrewHole("Screw_TR", halfWidth - 0.5, halfHeight - 0.5, halfDepth);
      AddScrewHole("Screw_BL", -halfWidth + 0.5, -halfHeight + 0.5, halfDepth);
      AddScrewHole("Screw_BR", halfWidth - 0.5, -halfHeight + 0.5, halfDepth);
      
      GetTreeViewNodeTitle().WriteSuccess();
   }
   
   private void AddSignalBar(string name, double xPos, double yPos, double zPos, double barHeight, string color)
   {
      // Signal strength bar - vertical bar indicator
      var bar = new FoShape3D($"{name}_Bar");
      bar.CreateBox($"{name}_Bar", 0.15, barHeight, 0.1);
      bar.Color = color;
      bar.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(bar);
   }
   
   private void AddPowerSocket(string name, double xPos, double yPos, double zPos)
   {
      // Power barrel jack connector (recessed into chassis)
      var socket = new FoShape3D($"{name}_Socket");
      socket.CreateBox($"{name}_Socket", 0.8, 0.8, 0.4);
      socket.Color = "black";
      socket.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(socket);
      
      // Inner contact
      var contact = new FoShape3D($"{name}_Contact");
      contact.CreateCylinder($"{name}_Contact", 0.2, 0.2, 0.3);
      contact.Color = "gold";
      contact.Transform = new Transform3($"{name}_ContactTransform")
      {
         Position = new Vector3(xPos, yPos, zPos),
         Rotation = new Euler(0, 90 * Math.PI / 180, 0)
      };
      AddShape(contact);
   }
   
   private void AddSIMHolder(string name, double xPos, double yPos, double zPos)
   {
      // SIM card slot (wider, flatter)
      var holder = new FoShape3D($"{name}_Holder");
      holder.CreateBox($"{name}_Holder", 2.0, 1.2, 0.3);
      holder.Color = "black";
      holder.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(holder);
      
      // SIM card outline
      var card = new FoShape3D($"{name}_Card");
      card.CreateBox($"{name}_Card", 1.5, 0.9, 0.05);
      card.Color = "darkgoldenrod";
      card.Transform = new Transform3($"{name}_CardTransform")
      {
         Position = new Vector3(xPos, yPos, zPos + 0.2)
      };
      AddShape(card);
   }
   
   private void AddRJ45Port(string name, double xPos, double yPos, double zPos, string label)
   {
      // RJ45 Ethernet port body (more realistic proportions)
      var port = new FoShape3D($"{name}_Port");
      port.CreateBox($"{name}_Port", 1.4, 1.2, 1.2);
      port.Color = "silver";
      port.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(port);
      
      // Port opening (recessed)
      var opening = new FoShape3D($"{name}_Opening");
      opening.CreateBox($"{name}_Opening", 0.3, 1.0, 0.9);
      opening.Color = "black";
      opening.Transform = new Transform3($"{name}_OpeningTransform")
      {
         Position = new Vector3(xPos + 0.6, yPos, zPos)
      };
      AddShape(opening);
      
      // Port contacts (gold pins visible)
      var contacts = new FoShape3D($"{name}_Contacts");
      contacts.CreateBox($"{name}_Contacts", 0.15, 0.8, 0.7);
      contacts.Color = "gold";
      contacts.Transform = new Transform3($"{name}_ContactsTransform")
      {
         Position = new Vector3(xPos + 0.65, yPos, zPos)
      };
      AddShape(contacts);
   }
   
   private void AddLED(string name, double xPos, double yPos, double zPos, string color, string label)
   {
      // LED indicator (small cylinder)
      var led = new FoShape3D($"{name}_LED");
      led.CreateCylinder($"{name}_LED", 0.12, 0.12, 0.08);
      led.Color = color;
      led.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(led);
      
      // LED label (smaller text)
      var ledLabel = new FoText3D($"{name}_Label")
      {
         Text = label,
         FontSize = 0.18,
         Transform = new Transform3($"{name}_LabelTransform")
         {
            Position = new Vector3(xPos, yPos - 0.6, zPos),
         },
         Color = "white"
      };
      AddShape(ledLabel);
   }
   
   private void AddScrewHole(string name, double xPos, double yPos, double zPos)
   {
      // Mounting screw hole
      var hole = new FoShape3D($"{name}_Hole");
      hole.CreateCylinder($"{name}_Hole", 0.15, 0.15, 0.1);
      hole.Color = "black";
      hole.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(xPos, yPos, zPos)
      };
      AddShape(hole);
   }
}
