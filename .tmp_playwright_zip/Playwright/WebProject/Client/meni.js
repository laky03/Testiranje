export class Meni {
    constructor(menuDiv) {
        this.menuDiv = menuDiv;
        this.url = "http://localhost:5138";
    }

    removeAllChildren(container) {
        while (container.firstChild) {
            container.removeChild(container.firstChild);
        }
    }

    show(show) {
        if (show) {
            this.menuDiv.classList.remove("hide");
        }
        else {
            this.menuDiv.classList.add("hide");
        }
    }

    promeniMeni(jelo) {
        if (jelo) {
            this.menuDiv.querySelector("h2").textContent = "Meni jela:";
        }
        else {
            this.menuDiv.querySelector("h2").textContent = "Meni pića:";
        }
    }

    async dodajUMeni(restoran, container, id, naziv, kalorijskaVrednost, cN) {
        const data = await fetch(`${this.url}/Sastojak/CenaProizvoda/${id}`).then(p => p.text());
        const cena = parseFloat(data);
        
        if (cena <= 0) {
            alert(data);
            return;
        }

        const li = document.createElement("li");
        li.classList.add(cN);
        const span = document.createElement("span");
        span.textContent = `${naziv} (${kalorijskaVrednost} kcal)`;
        const buttonSpan = document.createElement("span");
        buttonSpan.classList.add("poruci-span");
        const cenaP = document.createElement("p");
        cenaP.classList.add("cena-p");
        cenaP.textContent = `Cena: ${(400 + cena * 1.2).toFixed(2)} RSD`;
        buttonSpan.appendChild(cenaP);
        const button = document.createElement("button");
        button.textContent = "Poruči";
        buttonSpan.appendChild(button);
        li.appendChild(span);
        li.appendChild(buttonSpan);
        this.menuList.appendChild(li);

        button.addEventListener("click", async () => {
            const poruci = await fetch(`${this.url}/Meni/NarucivanjeJela/${id}`);

            if (poruci.ok) {
                restoran.prihodi += (400 + cena * 1.2);
                restoran.rashodi += (400 + cena);
                restoran.zarada = restoran.prihodi - restoran.rashodi;

                const zarada = container.querySelector(".zarada-p");
                zarada.textContent = `Zarada: ${restoran.zarada.toFixed(2)} RSD`;
                alert(`Vaš račun je: ${(400 + cena * 1.2).toFixed(2)} RSD.\nPrijatno!`);
            }
            else {
                alert(await poruci.text());
            }
        });
    }

    isprazni() {
        this.removeAllChildren(this.menuList);
    }

    crtajMeni() {
        // Meni
        const menuTitle = document.createElement("h2");
        menuTitle.textContent = "Meni";
        this.menuList = document.createElement("ul");
        this.menuList.classList.add("menu-list");

        const menuSection = document.createElement("div");
        menuSection.classList.add("menu");
        menuSection.appendChild(menuTitle);
        menuSection.appendChild(this.menuList);
        this.menuDiv.appendChild(menuSection);
    }
}
