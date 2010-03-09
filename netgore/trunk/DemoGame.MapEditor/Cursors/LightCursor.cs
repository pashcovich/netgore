﻿using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DemoGame.MapEditor.Properties;
using Microsoft.Xna.Framework;
using NetGore;
using NetGore.EditorTools;
using NetGore.Graphics;
using Color=Microsoft.Xna.Framework.Graphics.Color;

namespace DemoGame.MapEditor
{
    sealed class LightCursor : EditorCursor<ScreenForm>
    {
        ILight _selectedLight;
        Vector2 _selectedLightOffset;
        string _toolTip = string.Empty;
        object _toolTipObj = null;
        Vector2 _toolTipPos;

        /// <summary>
        /// Gets the cursor's <see cref="Image"/>.
        /// </summary>
        public override Image CursorImage
        {
            get { return Resources.cursor_lights; }
        }

        /// <summary>
        /// When overridden in the derived class, handles when the delete button has been pressed.
        /// </summary>
        public override void PressDelete()
        {
            var light = Container.SelectedObjs.Focused as ILight;
            if (light != null && light.Tag == Container.Map)
            {
                Container.Map.RemoveLight(light);
                Container.DrawingManager.LightManager.Remove(light);
                Container.SelectedObjs.Clear();
            }
        }

        /// <summary>
        /// When overridden in the derived class, gets the name of the cursor.
        /// </summary>
        public override string Name
        {
            get { return "Select Light"; }
        }

        /// <summary>
        /// Gets the priority of the cursor on the toolbar. Lower values appear first.
        /// </summary>
        public override int ToolbarPriority
        {
            get { return 40; }
        }

        /// <summary>
        /// When overridden in the derived class, handles drawing the interface for the cursor, which is
        /// displayed over everything else. This can include the name of entities, selection boxes, etc.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="ISpriteBatch"/> to use to draw.</param>
        public override void DrawInterface(ISpriteBatch spriteBatch)
        {
            // If we have a light under the cursor or selected, use the SizeAll cursor
            if (_selectedLight != null || FindMouseOverLight() != null)
                Container.Cursor = Cursors.SizeAll;

            // Draw the tooltip
            if (_toolTipObj != null && !string.IsNullOrEmpty(_toolTip))
                spriteBatch.DrawStringShaded(Container.SpriteFont, _toolTip, _toolTipPos, Color.White, Color.Black);
        }

        /// <summary>
        /// Finds the <see cref="ILight"/> under the cursor.
        /// </summary>
        /// <returns>The <see cref="ILight"/> under the cursor, or null if no <see cref="ILight"/> is under the cursor.</returns>
        ILight FindMouseOverLight()
        {
            var cursorPos = Container.CursorPos;

            var closestLight = Container.Map.Lights.MinElementOrDefault(x => cursorPos.QuickDistance(x.Center));
            if (closestLight == null)
                return null;

            if (cursorPos.QuickDistance(closestLight.Position) > 5)
                return null;

            return closestLight;
        }

        /// <summary>
        /// When overridden in the derived class, handles when a mouse button has been pressed.
        /// </summary>
        /// <param name="e">Mouse events.</param>
        public override void MouseDown(MouseEventArgs e)
        {
            _selectedLight = FindMouseOverLight();
            if (_selectedLight != null)
                _selectedLightOffset = Container.CursorPos - _selectedLight.Center;

            Container.SelectedObjs.SetSelected(_selectedLight);

            _toolTipObj = null;
        }

        /// <summary>
        /// When overridden in the derived class, handles when the cursor has moved.
        /// </summary>
        /// <param name="e">Mouse events.</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (_selectedLight != null)
            {
                // Move the light if dragging one
                _selectedLight.Teleport(Container.CursorPos - _selectedLightOffset);

                _toolTipObj = null;
            }
            else
                UpdateToolTip();
        }

        /// <summary>
        /// When overridden in the derived class, handles when a mouse button has been released.
        /// </summary>
        /// <param name="e">Mouse events.</param>
        public override void MouseUp(MouseEventArgs e)
        {
            _selectedLight = null;
            UpdateToolTip();
        }

        void UpdateToolTip()
        {
            if (_selectedLight != null)
            {
                _toolTipObj = null;
                _toolTip = null;
                return;
            }

            // Display the tooltip for the light under the cursor
            var light = FindMouseOverLight();
            if (_toolTipObj == light)
                return;

            _toolTipObj = light;
            if (light == null)
                return;

            _toolTip = string.Format("{0}\n{1} ({2}x{3})", light, light.Position, light.Size.X, light.Size.Y);
            _toolTipPos = EntityCursor.GetToolTipPos(Container.SpriteFont, _toolTip, light);
            _toolTipPos.X = light.Position.X + 5;
        }
    }
}