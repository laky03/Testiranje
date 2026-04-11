export class Restoran {
    constructor(restaurantDiv, menuDiv, meni, id, x, y, naziv, zbirOcena, brojOcena, prosecnaOcena, prihodi, rashodi, zarada) {
        this.restaurantDiv = restaurantDiv;
        this.menuDiv = menuDiv;
        this.meni = meni;
        this.id = id;
        this.x = x;
        this.y = y;
        this.naziv = naziv;
        this.zbirOcena = zbirOcena;
        this.brojOcena = brojOcena;
        this.prosecnaOcena = prosecnaOcena;
        this.prihodi = prihodi;
        this.rashodi = rashodi;
        this.zarada = zarada;
        this.url = "http://localhost:5138";
    }

    async crtaj() {
        this.card = document.createElement("div");
        this.card.className = "card";
        const naziv = document.createElement("h3");
        naziv.textContent = this.naziv;
        this.card.appendChild(naziv);

        const rating = document.createElement("p");
        rating.classList.add("ocena-p");
        rating.textContent = `Ocena: ${this.prosecnaOcena.toFixed(1)}`;
        this.card.appendChild(rating);

        const zarada = document.createElement("p");
        zarada.textContent = `Zarada: ${this.zarada.toFixed(2)} RSD`;
        zarada.classList.add("zarada-p");
        this.card.appendChild(zarada);

        // Ocena
        const rateForm = document.createElement("form");

        const ocena = document.createElement("select");
        ocena.required = true;
        ocena.classList.add("ocena-restorana");
        const naslov = document.createElement("h3");
        naslov.textContent = "Oceni restoran:";
        rateForm.appendChild(naslov);

        for (let i = 1; i <= 10; i++) {
            const option = document.createElement("option");
            option.textContent = i.toString();
            option.value = i;
            ocena.appendChild(option);
        }

        rateForm.appendChild(ocena);
        const dodajOcenu = document.createElement("button");
        dodajOcenu.type = "submit";
        dodajOcenu.textContent = "Oceni";
        rateForm.appendChild(dodajOcenu);

        this.card.appendChild(rateForm);

        const meniJela = document.createElement("button");
        const meniPica = document.createElement("button");

        meniJela.textContent = "Meni jela";
        meniPica.textContent = "Meni pića";

        meniJela.classList.add("meni-button");
        meniPica.classList.add("meni-button");

        this.card.appendChild(meniJela);
        this.card.appendChild(meniPica);

        meniJela.addEventListener("click", async () => {
            this.meni.isprazni();
            const menu = await fetch(`${this.url}/Restoran/VratiMenijeRestorana/${this.id}`);

            if (menu.ok) {
                this.meni.promeniMeni(true);

                (await menu.json()).meniJela.forEach(async item => {
                    await this.meni.dodajUMeni(this, this.card, item.id, item.naziv, item.kalorijskaVrednost, "food");
                });

                this.meni.show(true);
            }
            else {
                alert(await menu.text());
            }
        });

        meniPica.addEventListener("click", async () => {
            this.meni.isprazni();
            const menu = await fetch(`${this.url}/Restoran/VratiMenijeRestorana/${this.id}`);

            if (menu.ok) {
                this.meni.promeniMeni(false);

                (await menu.json()).meniPica.forEach(async item => {
                    await this.meni.dodajUMeni(this, this.card, item.id, item.naziv, item.kalorijskaVrednost, "drink");
                });

                this.meni.show(true);
            }
            else {
                alert(await menu.text());
            }
        });

        this.restaurantDiv.appendChild(this.card);

        // Oceni server
        rateForm.addEventListener("submit", async e => {
            e.preventDefault();
            const grade = e.target.querySelector(".ocena-restorana").value;

            const res = await fetch(`${this.url}/Restoran/OcenjivanjeRestorana/${this.id}/${grade}`, {
                method: "POST"
            });

            if (res.ok) {
                this.zbirOcena += parseInt(grade);
                this.brojOcena++;
                this.prosecnaOcena = this.zbirOcena / this.brojOcena;
                this.card.querySelector(".ocena-p").textContent = `Ocena: ${this.prosecnaOcena.toFixed(1)}`;
            } else {
                alert(await res.text());
            }

            rateForm.reset();
        });
    }
}
