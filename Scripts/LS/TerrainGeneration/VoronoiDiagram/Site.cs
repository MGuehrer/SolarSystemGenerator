using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS.TerrainGeneration;
using Circle.Delaunay;
using Circle.Shapes;
using UnityEngine;

namespace csDelaunay {

	public class Site : ICoord {

		private static Queue<Site> pool = new Queue<Site>();
	    private Rectf parentBorders;

		public static Site Create(Vector2f p, int index, float weigth, Rectf borders) {
			if (pool.Count > 0) {
				return pool.Dequeue().Init(p, index, weigth);
			} else {
				return new Site(p, index, weigth, borders);
			}
		}

		public static void SortSites(List<Site> sites) {
			sites.Sort(delegate(Site s0, Site s1) {
				int returnValue = Voronoi.CompareByYThenX(s0,s1);
				
				int tempIndex;
				
				if (returnValue == -1) {
					if (s0.siteIndex > s1.SiteIndex) {
						tempIndex = s0.SiteIndex;
						s0.SiteIndex = s1.SiteIndex;
						s1.SiteIndex = tempIndex;
					}
				} else if (returnValue == 1) {
					if (s1.SiteIndex > s0.SiteIndex) {
						tempIndex = s1.SiteIndex;
						s1.SiteIndex = s0.SiteIndex;
						s0.SiteIndex = tempIndex;
					}
				}
				
				return returnValue;
			});
		}

		public int Compare(Site s1, Site s2) {
			return s1.CompareTo(s2);
		}

		public int CompareTo(Site s1) {
			int returnValue = Voronoi.CompareByYThenX(this,s1);

			int tempIndex;

			if (returnValue == -1) {
				if (this.siteIndex > s1.SiteIndex) {
					tempIndex = this.SiteIndex;
					this.SiteIndex = s1.SiteIndex;
					s1.SiteIndex = tempIndex;
				}
			} else if (returnValue == 1) {
				if (s1.SiteIndex > this.SiteIndex) {
					tempIndex = s1.SiteIndex;
					s1.SiteIndex = this.SiteIndex;
					this.SiteIndex = tempIndex;
				}
			}

			return returnValue;
		}
		
		private const float EPSILON = 0.005f;
		private static bool CloseEnough(Vector2f p0, Vector2f p1) {
			return (p0-p1).magnitude < EPSILON;
		}
		
		private int siteIndex;
		public int SiteIndex {get{return siteIndex;} set{siteIndex=value;}}
		
		private Vector2f coord;
		public Vector2f Coord {get{return coord;}set{coord=value;}}

		public float x {get{return coord.x;}}
		public float y {get{return coord.y;}}

		private float weigth;
		public float Weigth {get{return weigth;}}

		// The edges that define this Site's Voronoi region:
		private List<Edge> edges;

	    public List<Edge> Edges
	    {
	        get
	        {
	            ReorderEdges();

                return edges;
	        }
	    }

	    // which end of each edge hooks up with the previous edge in edges:
		private List<LR> edgeOrientations;
		// ordered list of points that define the region clipped to bounds:
		private List<Vector2f> region;

		public Site(Vector2f p, int index, float weigth, Rectf borders)
        {
			Init(p, index, weigth);
            FalseNeighbours = new List<Site>();
            parentBorders = borders;
        }

		private Site Init(Vector2f p, int index, float weigth) {
			coord = p;
			siteIndex = index;
			this.weigth = weigth;
			edges = new List<Edge>();
			region = null;

			return this;
		}

		public override string ToString() {
			return "Site " + siteIndex + ": " + coord;
		}

	    public bool IsBorderSite
	    {
	        get
	        {
	            return OrderedColourPoints.Any(p => Math.Abs(p.Position.x - parentBorders.x) < 0.1f
	                                                || Math.Abs(p.Position.x - parentBorders.width - parentBorders.x) < 0.1f
	                                                || Math.Abs(p.Position.y - parentBorders.y) < 0.1f
	                                                || Math.Abs(p.Position.y - parentBorders.height - parentBorders.x) <
	                                                0.1f);
	        }
	    }

	    public bool IsNorthPoleSite
	    {
	        get { return OrderedColourPoints.Any(p => Math.Abs(p.Position.y - parentBorders.y) < 0.1f); }
	    }

	    public bool IsSouthPoleSite
	    {
	        get
	        {
	            return OrderedColourPoints.Any(p => Math.Abs(p.Position.y - parentBorders.height - parentBorders.x) < 0.1f);
	        }
	    }

        public bool IsBorderedOnSeamSite
	    {
	        get
	        {
	            return OrderedColourPoints.Any(p => Math.Abs(p.Position.x - parentBorders.x) < 0.1f
	                                                || Math.Abs(p.Position.x - parentBorders.width - parentBorders.x) < 0.1f);
	        }
	    }

        private List<TexturePoint> orderedColourPoints;

	    public List<TexturePoint> OrderedColourPoints
	    {
	        get
	        {
	            if (orderedColourPoints == null || orderedColourPoints.Count == 0)
	            {
	                // Get the list of points (only use one per spot)
	                var points = new Dictionary<TexturePoint, float>();
	                foreach (var edge in Edges)
	                {
	                    if (edge.Visible())
	                    {
	                        var p1 = edge.ClippedEnds[LR.LEFT];
	                        var p2 = edge.ClippedEnds[LR.RIGHT];

	                        var c1 = GetColourPointForCorner(p1);
	                        var c2 = GetColourPointForCorner(p2);

	                        if (!points.ContainsKey(c1))
	                            points.Add(c1, GetAngleBetweenTwoPoints(Coord.ToPoint(), c1.Position));
	                        if (!points.ContainsKey(c2))
	                            points.Add(c2, GetAngleBetweenTwoPoints(Coord.ToPoint(), c2.Position));
	                    }
	                }

	                orderedColourPoints = points.OrderBy(p => p.Value).Select(po => po.Key).ToList();
                }


	            orderedColourPoints =
	                orderedColourPoints.OrderBy(p => GetAngleBetweenTwoPoints(Coord.ToPoint(), p.Position)).ToList();
                return orderedColourPoints;
	        }
	    }

	    public IEnumerable<TexturePoint> PointsOnBorder
	    {
	        get { return OrderedColourPoints.Where(p => IsPointOnBorder(p.Position, parentBorders)).OrderBy(point=>point.Position.y); }
	    }

	    public bool IsPointOnBorder(Point p, Rectf border)
	    {
	        return Math.Abs(p.x - border.x) < 0.1f
	               || Math.Abs(p.x - border.width - border.x) < 0.1f;
	    }

	    // TODO This can be pushed into a helper function.
        public static float GetAngleBetweenTwoPoints(Point p1, Point p2)
	    {
	        float xDiff = p2.x - p1.x;
	        float yDiff = p2.y - p1.y;
	        return (float) (Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
        }

		private void Move(Vector2f p) {
			Clear();
			coord = p;
		}

		public void Dispose() {
			Clear();
			pool.Enqueue(this);
		}

		private void Clear() {
			if (edges != null) {
				edges.Clear();
				edges = null;
			}
			if (edgeOrientations != null) {
				edgeOrientations.Clear();
				edgeOrientations = null;
			}
			if (region != null) {
				region.Clear();
				region = null;
			}
		}

		public void AddEdge(Edge edge) {
			edges.Add(edge);
		}

		public Edge NearestEdge() {
			edges.Sort(Edge.CompareSitesDistances);
			return edges[0];
		}

	    public List<Vector2f> GetBorderPoints()
	    {
	        //var leftEdge = Graph.PlotBounds.x + (Graph.PlotBounds.width * 0.1);
	        //var rightEdge = Graph.PlotBounds.x + (Graph.PlotBounds.width * 0.9);
	        //var topEdge = Graph.PlotBounds.y + (Graph.PlotBounds.height * 0.1);
	        //var bottomEdge = Graph.PlotBounds.y + (Graph.PlotBounds.height * 0.9);
            
	        var points = new List<Vector2f>();
	        foreach (var edge in edges)
	        {
	            if (edge.ClippedEnds == null) continue;
	            Vector2f p1 = edge.ClippedEnds[LR.LEFT];
	            Vector2f p2 = edge.ClippedEnds[LR.RIGHT];
                
	            if (Math.Abs(p1.x - parentBorders.x) < 0.1f ||
	                Math.Abs(p1.x - (parentBorders.x + parentBorders.width)) < 0.1f) points.Add(p1);
	            if (Math.Abs(p2.x - parentBorders.x) < 0.1f ||
	                Math.Abs(p2.x - (parentBorders.x + parentBorders.width)) < 0.1f) points.Add(p2);

	            if (Math.Abs(p1.y - parentBorders.y) < 0.1f ||
	                Math.Abs(p1.y - (parentBorders.y + parentBorders.height)) < 0.1f) points.Add(p1);
	            if (Math.Abs(p2.y - parentBorders.y) < 0.1f ||
	                Math.Abs(p2.y - (parentBorders.y + parentBorders.height)) < 0.1f) points.Add(p2);
	        }

	        return points.Count == 2 ? points : null;
	    }

	    public List<Site> GetNeighboursFromEdgeCorner(Vector2f corner)
	    {
	        var list = new HashSet<Site>();
	        foreach (var site in GetAllNeighbors())
	        {
	            foreach (var edge in site.Edges)
	            {
	                if (!edge.Visible()) continue;

	                var p1 = edge.ClippedEnds[LR.LEFT];
	                var p2 = edge.ClippedEnds[LR.RIGHT];

	                if (p1.Equals(corner) || p2.Equals(corner))
	                {
	                    list.Add(site);
	                    break;
	                }
	            }
	        }
	        return list.ToList();
	    }

        public TexturePoint GetColourPointForCorner(Vector2f corner)
	    {
	        var sites = GetNeighboursFromEdgeCorner(corner);
            sites.Add(this);

	        var averageWeight = sites.Aggregate(0.0f, (current, s) => current + s.MapWeight);
	        averageWeight /= sites.Count;
	        var w = (int)Math.Round(averageWeight);

	        var point = corner.ToPoint();
	        return new TexturePoint(point.x, point.y, w);
	    }

        public Site GetNeighborFromEdge(Edge edge)
	    {
	        var neighbor = GetAllNeighbors().FirstOrDefault(n => n.Edges.Contains(edge));

	        return neighbor;
	    }

	    public void SmoothHeightMap()
	    {
	        var allNeighbours = GetAllNeighbors();
	        if (!allNeighbours.Any()) return;

	        var sum = allNeighbours.Sum(site => site.MapWeight);
	        sum /= allNeighbours.Count;

	        MapWeight = sum;
	    }

	    private int _mapWeight = -1;
	    public int MapWeight
	    {
	        get
	        {
	            if (_mapWeight == -1)
	            {
	                _mapWeight = GetHeightMapWeight();
	            }

	            return _mapWeight;
	        }
	        set { _mapWeight = value; }
	    }

	    private int GetHeightMapWeight()
	    {
	        if (!NeighborSites().Any()) return 0;
	        var sum = (from site in NeighborSites()
	            let dist = (int) site.coord.DistanceSquare(this.coord)
	            where dist <= 1000
	            select (int) site.coord.DistanceSquare(this.coord)).Sum() * 0.2f;
	        sum /= NeighborSites().Count;
	        if (sum > 255) sum = 255;

	        return (int)Math.Round(sum);
	    }

		public List<Site> NeighborSites()
        {
			if (edges == null || edges.Count == 0) {
				return new List<Site>();
			}
			if (edgeOrientations == null) {
				ReorderEdges();
			}
			List<Site> list = new List<Site>();
			foreach (Edge edge in edges.Where(e=>e.Visible()))
            {
				list.Add(NeighborSite(edge));
			}
			return list;
		}

		private Site NeighborSite(Edge edge) {
			if (this == edge.LeftSite) {
				return edge.RightSite;
			}
			if (this == edge.RightSite) {
				return edge.LeftSite;
			}
			return null;
		}

	    public List<Site> FalseNeighbours { get; set; }

	    public List<Site> GetAllNeighbors()
	    {
	        var l = new List<Site>();
            l.AddRange(NeighborSites());
            l.AddRange(FalseNeighbours);
	        return l;
	    }

		public List<Vector2f> Region(Rectf clippingBounds) {
			if (edges == null || edges.Count == 0) {
				return new List<Vector2f>();
			}
			if (edgeOrientations == null) {
				ReorderEdges();
				region = ClipToBounds(clippingBounds);
				if ((new Polygon(region)).PolyWinding() == Winding.CLOCKWISE) {
					region.Reverse();
				}
			}
			return region;
		}

		private void ReorderEdges()
        {
			EdgeReorderer reorderer = new EdgeReorderer(edges, typeof(Vertex));
			edges = reorderer.Edges;
			edgeOrientations = reorderer.EdgeOrientations;
			reorderer.Dispose();
		}

		private List<Vector2f> ClipToBounds(Rectf bounds) {
			List<Vector2f> points = new List<Vector2f>();
			int n = edges.Count;
			int i = 0;
			Edge edge;

			while (i < n && !edges[i].Visible()) {
				i++;
			}

			if (i == n) {
				// No edges visible
				return new List<Vector2f>();
			}
			edge = edges[i];
			LR orientation = edgeOrientations[i];
			points.Add(edge.ClippedEnds[orientation]);
			points.Add(edge.ClippedEnds[LR.Other(orientation)]);

			for (int j = i + 1; j < n; j++) {
				edge = edges[j];
				if (!edge.Visible()) {
					continue;
				}
				Connect(ref points, j, bounds);
			}
			// Close up the polygon by adding another corner point of the bounds if needed:
			Connect(ref points, i, bounds, true);

			return points;
		}

		private void Connect(ref List<Vector2f> points, int j, Rectf bounds, bool closingUp = false) {
			Vector2f rightPoint = points[points.Count-1];
			Edge newEdge = edges[j];
			LR newOrientation = edgeOrientations[j];

			// The point that must be conected to rightPoint:
			Vector2f newPoint = newEdge.ClippedEnds[newOrientation];

			if (!CloseEnough(rightPoint, newPoint)) {
				// The points do not coincide, so they must have been clipped at the bounds;
				// see if they are on the same border of the bounds:
				if (rightPoint.x != newPoint.x && rightPoint.y != newPoint.y) {
					// They are on different borders of the bounds;
					// insert one or two corners of bounds as needed to hook them up:
					// (NOTE this will not be correct if the region should take up more than
					// half of the bounds rect, for then we will have gone the wrong way
					// around the bounds and included the smaller part rather than the larger)
					int rightCheck = BoundsCheck.Check(rightPoint, bounds);
					int newCheck = BoundsCheck.Check(newPoint, bounds);
					float px, py;
					if ((rightCheck & BoundsCheck.RIGHT) != 0) {
						px = bounds.right;

						if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							py = bounds.bottom;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							py = bounds.top;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height) {
								py = bounds.top;
							} else {
								py = bounds.bottom;
							}
							points.Add(new Vector2f(px,py));
							points.Add(new Vector2f(bounds.left, py));
						}
					} else if ((rightCheck & BoundsCheck.LEFT) != 0) {
						px = bounds.left;

						if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							py = bounds.bottom;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							py = bounds.top;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.RIGHT) != 0) {
							if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height) {
								py = bounds.top;
							} else {
								py = bounds.bottom;
							}
							points.Add(new Vector2f(px,py));
							points.Add(new Vector2f(bounds.right, py));
						}
					} else if ((rightCheck & BoundsCheck.TOP) != 0) {
						py = bounds.top;

						if ((newCheck & BoundsCheck.RIGHT) != 0) {
							px = bounds.right;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							px = bounds.left;
							points.Add(new Vector2f(px,py));

						} else if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width) {
								px = bounds.left;
							} else {
								px = bounds.right;
							}
							points.Add(new Vector2f(px,py));
							points.Add(new Vector2f(px, bounds.bottom));
						}
					} else if ((rightCheck & BoundsCheck.BOTTOM) != 0) {
						py = bounds.bottom;
						
						if ((newCheck & BoundsCheck.RIGHT) != 0) {
							px = bounds.right;
							points.Add(new Vector2f(px,py));
							
						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							px = bounds.left;
							points.Add(new Vector2f(px,py));
							
						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width) {
								px = bounds.left;
							} else {
								px = bounds.right;
							}
							points.Add(new Vector2f(px,py));
							points.Add(new Vector2f(px, bounds.top));
						}
					}
				}
				if (closingUp) {
					// newEdge's ends have already been added
					return;
				}
				points.Add(newPoint);
			}
			Vector2f newRightPoint = newEdge.ClippedEnds[LR.Other(newOrientation)];
			if (!CloseEnough(points[0], newRightPoint)) {
				points.Add(newRightPoint);
			}
		}

		public float Dist(ICoord p) {
			return (this.Coord - p.Coord).magnitude;
		}


        // TECTONIC STUFF
        public TectonicPlate TectonicPlate { get; set; }

	    public IEnumerable<Site> GetNeighbouringTectonicPlateSites()
	    {
            return GetAllNeighbors().Where(n=>n.TectonicPlate != this.TectonicPlate);
	    }
    }

	public class BoundsCheck {
		public const int TOP = 1;
		public const int BOTTOM = 2;
		public const int LEFT = 4;
		public const int RIGHT = 8;

		/*
		 * 
		 * @param point
		 * @param bounds
		 * @return an int with the appropriate bits set if the Point lies on the corresponding bounds lines
		 */
		public static int Check(Vector2f point, Rectf bounds) {
			int value = 0;
			if (point.x == bounds.left) {
				value |= LEFT;
			}
			if (point.x == bounds.right) {
				value |= RIGHT;
			}
			if (point.y == bounds.top) {
				value |= TOP;
			}
			if (point.y == bounds.bottom) {
				value |= BOTTOM;
			}

			return value;
		}
	}
}
