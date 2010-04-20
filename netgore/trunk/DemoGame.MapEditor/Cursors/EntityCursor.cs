using System;
using System.Linq;
using System.Windows.Forms;
using DemoGame.MapEditor.Properties;
using NetGore;
using NetGore.EditorTools;
using NetGore.Graphics;
using SFML.Graphics;
using Image = System.Drawing.Image;

namespace DemoGame.MapEditor
{
    sealed class EntityCursor : EditorCursor<ScreenForm>
    {
        readonly ContextMenu _contextMenu;
        readonly MenuItem _mnuIgnoreWalls;

        Vector2 _selectionOffset;
        string _toolTip = string.Empty;
        object _toolTipObject = null;
        Vector2 _toolTipPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCursor"/> class.
        /// </summary>
        public EntityCursor()
        {
            _mnuIgnoreWalls = new MenuItem("Ignore Walls", Menu_IgnoreWalls_Click) { Checked = true };
            _contextMenu = new ContextMenu(new MenuItem[] { _mnuIgnoreWalls });
        }

        /// <summary>
        /// Gets the cursor's <see cref="System.Drawing.Image"/>.
        /// </summary>
        public override Image CursorImage
        {
            get { return Resources.cursor_entities; }
        }

        /// <summary>
        /// When overridden in the derived class, gets the name of the cursor.
        /// </summary>
        public override string Name
        {
            get { return "Select Entity"; }
        }

        /// <summary>
        /// Gets the priority of the cursor on the toolbar. Lower values appear first.
        /// </summary>
        public override int ToolbarPriority
        {
            get { return 0; }
        }

        /// <summary>
        /// When overridden in the derived class, handles drawing the interface for the cursor, which is
        /// displayed over everything else. This can include the name of entities, selection boxes, etc.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="ISpriteBatch"/> to use to draw.</param>
        public override void DrawInterface(ISpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_toolTip))
                spriteBatch.DrawStringShaded(Container.RenderFont, _toolTip, _toolTipPos, Color.White, Color.Black);
        }

        /// <summary>
        /// When overridden in the derived class, gets the <see cref="ContextMenu"/> used by this cursor
        /// to display additional functions and settings.
        /// </summary>
        /// <returns>
        /// The <see cref="ContextMenu"/> used by this cursor to display additional functions and settings,
        /// or null for no <see cref="ContextMenu"/>.
        /// </returns>
        public override ContextMenu GetContextMenu()
        {
            return _contextMenu;
        }

        Entity GetEntityUnderCursor(ScreenForm screen)
        {
            var cursorPos = screen.CursorPos;
            return screen.Map.Spatial.Get<Entity>(cursorPos, GetEntityUnderCursorFilter);
        }

        bool GetEntityUnderCursorFilter(Entity entity)
        {
            if (entity is CharacterEntity)
                return false;

            if (_mnuIgnoreWalls.Checked && entity is WallEntityBase)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the position to display the tooltip text.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The tooltip text.</param>
        /// <param name="entity">The entity the tooltip is for.</param>
        /// <returns>The position to display the tooltip text.</returns>
        public static Vector2 GetToolTipPos(Font font, string text, ISpatial entity)
        {
            var pos = new Vector2(entity.Max.X, entity.Position.Y);
            pos -= new Vector2(5, (font.CharacterSize * text.Split('\n').Length) + 5);
            return pos;
        }

        void Menu_IgnoreWalls_Click(object sender, EventArgs e)
        {
            _mnuIgnoreWalls.Checked = !_mnuIgnoreWalls.Checked;
        }

        /// <summary>
        /// When overridden in the derived class, handles when a mouse button has been pressed.
        /// </summary>
        /// <param name="e">Mouse events.</param>
        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // Set the selected entity to the first entity we find at the cursor
            Container.SelectedObjs.SetSelected(GetEntityUnderCursor(Container));

            // Set the offset
            var focusedEntity = Container.SelectedObjs.Focused as Entity;
            if (focusedEntity != null)
                _selectionOffset = Container.CursorPos - focusedEntity.Position;
        }

        /// <summary>
        /// When overridden in the derived class, handles when the cursor has moved.
        /// </summary>
        /// <param name="e">Mouse events.</param>
        public override void MouseMove(MouseEventArgs e)
        {
            // Get the map and ensure a valid cursor position
            var map = Container.Map;
            if (map == null || !map.IsInMapBoundaries(Container.CursorPos))
                return;

            var focusedEntity = Container.SelectedObjs.Focused as Entity;

            if (focusedEntity != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Container.KeyEventArgs.Control)
                    {
                        // Resize the entity
                        var size = Container.CursorPos - focusedEntity.Position;
                        if (size.X < 4)
                            size.X = 4;
                        if (size.Y < 4)
                            size.Y = 4;
                        map.SafeResizeEntity(focusedEntity, size);
                    }
                    else
                    {
                        // Move the entity
                        map.SafeTeleportEntity(focusedEntity, Container.CursorPos - _selectionOffset);
                    }
                }
            }
            else
            {
                // Set the tooltip to the entity under the cursor
                var hoverEntity = GetEntityUnderCursor(Container);

                if (hoverEntity == null)
                {
                    _toolTip = string.Empty;
                    _toolTipObject = null;
                }
                else if (_toolTipObject != hoverEntity)
                {
                    _toolTipObject = hoverEntity;
                    _toolTip = string.Format("{0}\n{1} ({2}x{3})", hoverEntity, hoverEntity.Position, hoverEntity.Size.X,
                                             hoverEntity.Size.Y);
                    _toolTipPos = GetToolTipPos(Container.RenderFont, _toolTip, hoverEntity);
                }
            }
        }

        /// <summary>
        /// When overridden in the derived class, handles when the delete button has been pressed.
        /// </summary>
        public override void PressDelete()
        {
            foreach (var selected in Container.SelectedObjs.SelectedObjects.OfType<Entity>())
            {
                selected.Dispose();
            }

            Container.SelectedObjs.Clear();
        }
    }
}