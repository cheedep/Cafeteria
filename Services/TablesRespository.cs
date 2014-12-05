using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cafeteria.Services.Contracts;
using Cafeteria.Services.Domain;

namespace Cafeteria.Services
{
    [Export(typeof(ITablesRepository))]
    public class TablesRepository : ITablesRepository
    {
        private readonly string _tablesPath;
        private readonly string _reservationsPath;

        private XElement _reservationXml;

        public TablesRepository()
        {
            string tablesFolder = ConfigurationManager.AppSettings["TablesFolder"];
            string tablesFileName = ConfigurationManager.AppSettings["TablesFileName"];
            string reservationsFileName = ConfigurationManager.AppSettings["ReservationsFileName"];

            _tablesPath = Path.Combine(tablesFolder, tablesFileName);
            _reservationsPath = Path.Combine(tablesFolder, reservationsFileName);

            LoadData();
        }

        private void LoadData()
        {

            var xElem = XElement.Load(_tablesPath);

            var tables = from elem in xElem.Descendants("Table")
                         select new Table
                         {
                             Id = Convert.ToInt32(elem.Attribute("Id").Value),
                             MaxOccupancy = Convert.ToInt32(elem.Attribute("MaxOccupancy").Value)
                         };

            Tables = tables.OrderBy(x => x.MaxOccupancy).ToList();

            if (File.Exists(_reservationsPath))
            {
                _reservationXml = XElement.Load(_reservationsPath);
                var reservations = from elem in _reservationXml.Descendants("Reservation")
                                   select new Reservation
                                   {
                                       Id = Convert.ToInt32(elem.Attribute("Id").Value),
                                       For = elem.Attribute("For").Value,
                                       FromTime = DateTime.Parse(elem.Attribute("FromTime").Value),
                                       PartySize = Convert.ToInt32(elem.Attribute("PartySize").Value),
                                       ToTime = DateTime.Parse(elem.Attribute("ToTime").Value),
                                       TableId = Convert.ToInt32(elem.Attribute("TableId").Value)
                                   };
                Reservations = reservations.ToList();
            }
            else
            {
                _reservationXml = new XElement("Reservations");
                Reservations = new List<Reservation>();
            }
        }

        private List<Table> Tables { get; set; }
        private List<Reservation> Reservations { get; set; }

        public System.Linq.IQueryable<Table> QueryTables()
        {
            return Tables.AsQueryable();
        }

        public System.Linq.IQueryable<Reservation> QueryReservations()
        {
            return Reservations.AsQueryable();
        }

        private Reservation ConvertModelTo(Models.Reservation reservation)
        {
            var r = new Reservation
            {
                Id = reservation.ReservationId,
                For = reservation.ReservedFor,
                PartySize = reservation.NumberOfPeople.Value,
                FromTime = reservation.FromTime,
                ToTime = reservation.ToTime,
                TableId = reservation.TableNumber.Value
            };
            return r;
        }

        private Models.Reservation ConvertToModel(Reservation reservation)
        {
            var r = new Models.Reservation
            {
                ReservationId = reservation.Id,
                ToTime = reservation.ToTime,
                FromTime = reservation.FromTime,
                NumberOfPeople = reservation.PartySize,
                ReservedFor = reservation.For,
                TableNumber = reservation.TableId
            };
            return r;
        }

        public bool AddOrUpdateReservation(Models.Reservation reservation)
        {
            if (reservation.ReservationId == 0) //new
            {
                reservation.ReservationId = GetId();
                var r = ConvertModelTo(reservation);
                AddReservationXml(r);
                Reservations.Add(r);
                return true;
            }

            //update
            var existing = Reservations.FirstOrDefault(x => x.Id == reservation.ReservationId);
            if (existing != null)
            {
                existing.For = reservation.ReservedFor;
                existing.PartySize = reservation.NumberOfPeople.Value;
                existing.FromTime = reservation.FromTime;
                existing.ToTime = reservation.ToTime;

                UpdateReservationXml(existing);

                return true;
            }

            return false;
        }

        private int GetId()
        {
            if (Reservations.Count == 0)
                return 1;
            return Reservations.Max(x => x.Id) + 1;
        }

        public bool DeleteReservation(Models.Reservation reservation)
        {
            var existing = Reservations.FirstOrDefault(x => x.Id == reservation.ReservationId);
            if (existing != null)
            {
                DeleteReservationXml(existing.Id);
                Reservations.Remove(existing);
                return true;
            }
            return false;
        }

        public List<Models.Reservation> GetAllReservations()
        {
            var models = new List<Models.Reservation>();
            foreach (var reservation in Reservations)
            {
                models.Add(ConvertToModel(reservation));
            }

            return models;
        }

        private void UpdateReservationXml(Reservation r)
        {
            XElement old = null;

            // Find the product element
            old = (from elem in _reservationXml.Descendants("Reservation")
                   where elem.Attribute("Id").Value ==
                      r.Id.ToString()
                   select elem).SingleOrDefault();

            if (old != null)
            {
                // Update the data
                old.Attribute("For").Value = r.For;
                old.Attribute("FromTime").Value = r.FromTime.ToString();
                old.Attribute("ToTime").Value = r.ToTime.ToString();
                old.Attribute("TableId").Value = r.TableId.ToString();
                old.Attribute("PartySize").Value = r.PartySize.ToString();
                SaveXml();
            }
        }

        private void AddReservationXml(Reservation r)
        {
            // Create new Product element
            var newElem = new XElement("Reservation",
              new XAttribute("Id", r.Id),
              new XAttribute("For", r.For),
              new XAttribute("FromTime", r.FromTime),
              new XAttribute("ToTime", r.ToTime),
              new XAttribute("PartySize", r.PartySize),
              new XAttribute("TableId", r.TableId));

            // Add to element collection
            _reservationXml.Add(newElem);

            SaveXml();
        }

        private void DeleteReservationXml(int id)
        {
            // Find the product element
            XElement old = (from elem in _reservationXml.Descendants("Reservation")
                            where elem.Attribute("Id").Value == id.ToString()
                            select elem).SingleOrDefault();

            if (old != null)
            {
                // Delete the element
                old.Remove();

                SaveXml();
            }
        }

        private void SaveXml()
        {
            _reservationXml.Save(_reservationsPath);
        }


        public List<Models.Reservation> GetAllReservationsSortedByTableAndTime()
        {
            return QueryReservations()
                        .OrderBy(x => x.TableId)
                        .ThenBy(x => x.FromTime)
                        .Select(x => ConvertToModel(x)).ToList();
        }


        public int GetBiggestTableSize()
        {
            return Tables.Max(x => x.MaxOccupancy);
        }

        public List<Table> GetAllDomainTablesSortedByMaxOccupancy()
        {
            return Tables;
        }

        public List<Reservation> GetAllDomainReservations()
        {
            return Reservations;
        }


        public List<Cafeteria.Models.Table> GetAllTables()
        {
            var query = from t in QueryTables()
                        select new Cafeteria.Models.Table { TableId = t.Id, MaxOccupancy = t.MaxOccupancy };

            return query.ToList();
        }
    }
}
