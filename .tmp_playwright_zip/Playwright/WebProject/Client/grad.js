import { Restoran } from "./restoran.js";
import { Meni } from "./meni.js";

export class Grad {
    constructor(container) {
        this.container = container;
        this.url = "http://localhost:5138";
    }

    removeAllChildren(container) {
        while (container.firstChild) {
            container.removeChild(container.firstChild);
        }
    }

    show(element, show) {
        if (show) {
            element.classList.remove("hide");
        }
        else {
            element.classList.add("hide");
        }
    }

    async crtaj() {
        // Gradovi select
        const cityLabel = document.createElement("label");
        cityLabel.textContent = "Odaberite grad:";
        const citySelect = document.createElement("select");
        const option = document.createElement("option");
        option.textContent = "Odaberite grad...";
        option.disabled = true;
        option.selected = true;
        citySelect.appendChild(option);
        this.container.appendChild(cityLabel);
        this.container.appendChild(citySelect);

        const filter = document.createElement("div");
        filter.classList.add("filter");
        this.show(filter, false);
        this.container.appendChild(filter);

        const div = document.createElement("form");
        div.classList.add("filter-div");
        filter.appendChild(div);

        const latSpan = document.createElement("span");
        latSpan.classList.add("full-size", "g-c1");
        div.appendChild(latSpan);

        const lonSpan = document.createElement("span");
        lonSpan.classList.add("full-size", "g-c2");
        div.appendChild(lonSpan);

        const distSpan = document.createElement("span");
        distSpan.classList.add("full-size", "g-c1");
        div.appendChild(distSpan);

        const tipSpan = document.createElement("span");
        tipSpan.classList.add("full-size", "g-c2");
        div.appendChild(tipSpan);

        const labLat = document.createElement("label");
        labLat.classList.add("filter");
        labLat.textContent = "Geografska širina (°): ";
        latSpan.appendChild(labLat);

        const lat = document.createElement("input");
        lat.required = true;
        lat.type = "number";
        lat.min = -90;
        lat.max = 90;
        lat.step = "any";
        latSpan.appendChild(lat);

        const labLon = document.createElement("label");
        labLon.classList.add("filter");
        labLon.textContent = "Geografska dužina (°): ";
        lonSpan.appendChild(labLon);

        const lon = document.createElement("input");
        lon.required = true;
        lon.type = "number";
        lon.min = -180;
        lon.max = 180;
        lon.step = "any";
        lonSpan.appendChild(lon);

        const labDist = document.createElement("label");
        labDist.classList.add("filter");
        labDist.textContent = "Udaljenost (m): ";
        distSpan.appendChild(labDist);

        const udaljenost = document.createElement("input");
        udaljenost.required = true;
        udaljenost.type = "number";
        udaljenost.min = 0;
        udaljenost.max = 1000000000;
        distSpan.appendChild(udaljenost);

        const labTip = document.createElement("label");
        labTip.classList.add("filter");
        labTip.textContent = "Tip hrane: ";
        tipSpan.appendChild(labTip);

        const tipHrane = document.createElement("select");
        tipHrane.required = true;
        tipHrane.classList.add("tip-hrane");

        const tipoviHrane = await fetch(`${this.url}/Restoran/TipoviRestorana`);

        if (tipoviHrane.ok) {
            (await tipoviHrane.json()).forEach(p => {
                const tip = document.createElement("option");
                tip.value = p;
                tip.textContent = p;
                tipHrane.appendChild(tip);
            });
        }
        else {
            alert(await tipoviHrane.text());
        }

        tipSpan.appendChild(tipHrane);

        const filterButton = document.createElement("button");
        filterButton.classList.add("filter-button");
        filterButton.type = "submit";
        filterButton.textContent = "Filtriraj";
        div.appendChild(filterButton);

        const title = document.createElement("h1");
        title.textContent = "Restorani";
        filter.appendChild(title);

        const restaurantsDiv = document.createElement("div");
        restaurantsDiv.classList.add("restaurants");
        this.container.appendChild(restaurantsDiv);
        this.restaurantsDiv = restaurantsDiv;

        const menuDiv = document.createElement("div");
        menuDiv.classList.add("menu-div");
        this.container.appendChild(menuDiv);
        this.menuDiv = menuDiv;

        const meni = new Meni(this.menuDiv);
        meni.crtajMeni();
        meni.show(false);

        // Gradovi fetch
        const cities = await fetch(`${this.url}/Grad/VratiGradoviInfo`);

        if (cities.ok) {
            (await cities.json()).forEach(city => {
                const option = document.createElement("option");
                option.value = city.identifikator;
                option.textContent = city.naziv;
                citySelect.appendChild(option);
            });
        }
        else {
            alert(await cities.text());
        }

        // Event za grad
        citySelect.addEventListener("change", async () => {
            const cityId = citySelect.value;

            this.show(filter, true);

            this.removeAllChildren(this.restaurantsDiv);
            meni.isprazni();
            meni.show(false);

            if (cityId > 0) {
                const data = await fetch(`${this.url}/Restoran/PreuzmiRestoraneGrada/${cityId}`);
                
                if (data.ok) {
                    const restorani = (await data.json())[0]?.restorani || [];
                    // [0] zbog Select-a sa serverske strane. Nema nigde FirstOrDefault ili slične
                    // metode. Iako se vraćaju samo rezultati u jednom gradu, vraćaju se kao lista.

                    restorani.forEach(restaurant => {
                        const restoran = new Restoran(restaurantsDiv, menuDiv, meni, restaurant.id, restaurant.x, restaurant.y, restaurant.naziv, restaurant.zbirOcena, restaurant.brojOcena, restaurant.prosecnaOcena, restaurant.prihodi, restaurant.rashodi, restaurant.zarada);
                        restoran.crtaj();
                    });
                }
                else {
                    alert(await data.text());
                }
            }
        });

        // Filter
        div.addEventListener("submit", async e => {
            e.preventDefault();
            const cityId = citySelect.value;

            this.removeAllChildren(this.restaurantsDiv);
            meni.isprazni();
            meni.show(false);

            if (cityId > 0) {
                const data = await fetch(`${this.url}/Restoran/PreuzmiRestoraneUBlizini/${lat.value}/${lon.value}/${udaljenost.value}/${tipHrane.value}`);
                
                if (data.ok) {
                    const restorani = (await data.json());

                    restorani.forEach(restaurant => {
                        const restoran = new Restoran(restaurantsDiv, menuDiv, meni, restaurant.id, restaurant.x, restaurant.y, restaurant.naziv, restaurant.zbirOcena, restaurant.brojOcena, restaurant.prosecnaOcena, restaurant.prihodi, restaurant.rashodi, restaurant.zarada);
                        restoran.crtaj();
                    });
                }
                else {
                    alert(await data.text());
                }
            }
        });
    }
}